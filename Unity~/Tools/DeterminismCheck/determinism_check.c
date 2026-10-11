// Cross-platform determinism probe for joltc.
// Drops a pile of rotated compound "pentomino-like" bodies onto a floor and
// prints an FNV-1a hash of every body's pose and velocity at fixed checkpoints.
// Identical output on every platform means the simulation is bit-exact.
//
// Build with -ffp-contract=off (or /fp:precise) so this file itself adds no FMA.

#include "joltc.h"
#include <inttypes.h>
#include <math.h>
#include <stdio.h>
#include <string.h>

#define PIECE_COUNT 48
#define STEP_COUNT 900
#define CHECKPOINT_INTERVAL 150

static uint64_t g_hash = 1469598103934665603ull;

static void HashBytes(const void* data, size_t size)
{
    const unsigned char* bytes = (const unsigned char*)data;
    for (size_t i = 0; i < size; i++)
    {
        g_hash ^= bytes[i];
        g_hash *= 1099511628211ull;
    }
}

static void HashFloat(float value)
{
    uint32_t bits;
    memcpy(&bits, &value, sizeof(bits));
    HashBytes(&bits, sizeof(bits));
}

// Only +, -, *, / and sqrt are used here, which IEEE 754 defines exactly.
static JPH_Quat MakeRotation(int index)
{
    float x = (float)((index * 7) % 11) - 5.0f;
    float y = (float)((index * 5) % 13) - 6.0f;
    float z = (float)((index * 3) % 7) - 3.0f;
    float w = (float)((index * 11) % 17) + 1.0f;
    float length = sqrtf(x * x + y * y + z * z + w * w);
    JPH_Quat q = { x / length, y / length, z / length, w / length };
    return q;
}

static JPH_Shape* CreatePieceShape(int variant)
{
    // Five unit cubes laid out in a few pentomino patterns.
    static const float layouts[3][5][3] = {
        { { 0, 0, 0 }, { 1, 0, 0 }, { 2, 0, 0 }, { 3, 0, 0 }, { 4, 0, 0 } },
        { { 0, 0, 0 }, { 1, 0, 0 }, { 2, 0, 0 }, { 2, 1, 0 }, { 2, 2, 0 } },
        { { 0, 0, 0 }, { 1, 0, 0 }, { 1, 1, 0 }, { 2, 1, 0 }, { 1, 0, 1 } },
    };

    JPH_StaticCompoundShapeSettings* settings = JPH_StaticCompoundShapeSettings_Create();
    JPH_Vec3 halfExtent = { 0.5f, 0.5f, 0.5f };
    for (int i = 0; i < 5; i++)
    {
        JPH_BoxShape* box = JPH_BoxShape_Create(&halfExtent, 0.0f);
        JPH_Vec3 center = { layouts[variant][i][0], layouts[variant][i][1], layouts[variant][i][2] };
        JPH_Quat identity = { 0.0f, 0.0f, 0.0f, 1.0f };
        JPH_CompoundShapeSettings_AddShape2((JPH_CompoundShapeSettings*)settings, &center, &identity, (const JPH_Shape*)box, 0);
        JPH_Shape_Destroy((JPH_Shape*)box);
    }

    JPH_Shape* shape = (JPH_Shape*)JPH_StaticCompoundShape_Create(settings);
    JPH_ShapeSettings_Destroy((JPH_ShapeSettings*)settings);
    return shape;
}

int main(void)
{
    if (!JPH_Init())
    {
        fprintf(stderr, "JPH_Init failed\n");
        return 1;
    }

    JPH_JobSystem* jobs = JPH_JobSystemThreadPool_Create(NULL);

    JPH_ObjectLayerPairFilter* pairs = JPH_ObjectLayerPairFilterTable_Create(2);
    JPH_ObjectLayerPairFilterTable_EnableCollision(pairs, 0, 1);
    JPH_ObjectLayerPairFilterTable_EnableCollision(pairs, 1, 1);
    JPH_BroadPhaseLayerInterface* broadPhase = JPH_BroadPhaseLayerInterfaceTable_Create(2, 2);
    JPH_BroadPhaseLayerInterfaceTable_MapObjectToBroadPhaseLayer(broadPhase, 0, 0);
    JPH_BroadPhaseLayerInterfaceTable_MapObjectToBroadPhaseLayer(broadPhase, 1, 1);
    JPH_ObjectVsBroadPhaseLayerFilter* objectVsBroadPhase =
        JPH_ObjectVsBroadPhaseLayerFilterTable_Create(broadPhase, 2, pairs, 2);

    JPH_PhysicsSystemSettings systemSettings;
    memset(&systemSettings, 0, sizeof(systemSettings));
    systemSettings.maxBodies = 4096;
    systemSettings.maxBodyPairs = 16384;
    systemSettings.maxContactConstraints = 16384;
    systemSettings.broadPhaseLayerInterface = broadPhase;
    systemSettings.objectLayerPairFilter = pairs;
    systemSettings.objectVsBroadPhaseLayerFilter = objectVsBroadPhase;
    JPH_PhysicsSystem* system = JPH_PhysicsSystem_Create(&systemSettings);

    JPH_PhysicsSettings physicsSettings;
    JPH_PhysicsSystem_GetPhysicsSettings(system, &physicsSettings);
    physicsSettings.deterministicSimulation = true;
    physicsSettings.useLargeIslandSplitter = false;
    JPH_PhysicsSystem_SetPhysicsSettings(system, &physicsSettings);

    JPH_Vec3 gravity = { 0.0f, -20.0f, 0.0f };
    JPH_PhysicsSystem_SetGravity(system, &gravity);

    JPH_BodyInterface* bodies = JPH_PhysicsSystem_GetBodyInterface(system);

    JPH_Vec3 floorHalfExtent = { 50.0f, 0.5f, 50.0f };
    JPH_BoxShape* floorShape = JPH_BoxShape_Create(&floorHalfExtent, 0.0f);
    JPH_RVec3 floorPosition = { 0.0f, -0.5f, 0.0f };
    JPH_BodyCreationSettings* floorSettings =
        JPH_BodyCreationSettings_Create3((const JPH_Shape*)floorShape, &floorPosition, NULL, JPH_MotionType_Static, 0);
    JPH_BodyInterface_CreateAndAddBody(bodies, floorSettings, JPH_Activation_DontActivate);
    JPH_BodyCreationSettings_Destroy(floorSettings);

    JPH_Shape* pieceShapes[3];
    for (int i = 0; i < 3; i++)
    {
        pieceShapes[i] = CreatePieceShape(i);
    }

    JPH_BodyID ids[PIECE_COUNT];
    for (int i = 0; i < PIECE_COUNT; i++)
    {
        JPH_RVec3 position = {
            (float)((i * 3) % 5) - 2.0f + 0.13f * (float)(i % 3),
            2.0f + 1.5f * (float)i,
            (float)((i * 2) % 5) - 2.0f - 0.07f * (float)(i % 4),
        };
        JPH_Quat rotation = MakeRotation(i);
        JPH_BodyCreationSettings* settings = JPH_BodyCreationSettings_Create3(
            pieceShapes[i % 3], &position, &rotation, JPH_MotionType_Dynamic, 1);
        JPH_BodyCreationSettings_SetFriction(settings, 0.6f);
        JPH_BodyCreationSettings_SetRestitution(settings, 0.0f);
        ids[i] = JPH_BodyInterface_CreateAndAddBody(bodies, settings, JPH_Activation_Activate);
        JPH_BodyCreationSettings_Destroy(settings);
    }

    const float deltaTime = 0.02f;
    for (int step = 1; step <= STEP_COUNT; step++)
    {
        if (JPH_PhysicsSystem_Update(system, deltaTime, 1, jobs) != JPH_PhysicsUpdateError_None)
        {
            fprintf(stderr, "update failed at step %d\n", step);
            return 1;
        }

        if (step % CHECKPOINT_INTERVAL != 0)
        {
            continue;
        }

        for (int i = 0; i < PIECE_COUNT; i++)
        {
            JPH_RVec3 position;
            JPH_Quat rotation;
            JPH_Vec3 linear;
            JPH_Vec3 angular;
            JPH_BodyInterface_GetPositionAndRotation(bodies, ids[i], &position, &rotation);
            JPH_BodyInterface_GetLinearVelocity(bodies, ids[i], &linear);
            JPH_BodyInterface_GetAngularVelocity(bodies, ids[i], &angular);
            HashFloat(position.x); HashFloat(position.y); HashFloat(position.z);
            HashFloat(rotation.x); HashFloat(rotation.y); HashFloat(rotation.z); HashFloat(rotation.w);
            HashFloat(linear.x); HashFloat(linear.y); HashFloat(linear.z);
            HashFloat(angular.x); HashFloat(angular.y); HashFloat(angular.z);
        }

        JPH_RVec3 sample;
        JPH_BodyInterface_GetPosition(bodies, ids[PIECE_COUNT - 1], &sample);
        printf("step %4d hash %016" PRIx64 " last.y %.9g\n", step, g_hash, sample.y);
    }

    printf("final %016" PRIx64 "\n", g_hash);

    JPH_JobSystem_Destroy(jobs);
    JPH_PhysicsSystem_Destroy(system);
    JPH_Shutdown();
    return 0;
}
