/* 단정밀도 64비트 C ABI의 컴파일 시 배치 확인. 고정한 joltc.h로 컴파일한다. */
#include "joltc.h"

_Static_assert(sizeof(void*) == 8, "64-bit pointers required");
_Static_assert(sizeof(bool) == 1, "C bool width");
_Static_assert(sizeof(JPH_ObjectLayer) == 4, "object layer width");
_Static_assert(sizeof(JPH_BroadPhaseLayer) == 1, "broad phase layer width");
_Static_assert(sizeof(JPH_BodyID) == 4, "body ID width");
_Static_assert(sizeof(JPH_Vec3) == 12, "Vec3 layout");
_Static_assert(sizeof(JPH_RVec3) == 12, "single precision position required");
_Static_assert(sizeof(JPH_Quat) == 16, "quaternion layout");
_Static_assert(sizeof(JPH_Triangle) == 40, "triangle layout");
_Static_assert(sizeof(JPH_PhysicsSystemSettings) == 48, "physics settings layout");
_Static_assert(offsetof(JPH_PhysicsSystemSettings, broadPhaseLayerInterface) == 24, "physics settings pointer offset");
_Static_assert(sizeof(JobSystemThreadPoolConfig) == 12, "thread pool config layout");
