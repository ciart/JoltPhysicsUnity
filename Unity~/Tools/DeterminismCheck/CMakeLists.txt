cmake_minimum_required(VERSION 3.16 FATAL_ERROR)

project(determinism_check C)

set(JOLTC_INCLUDE_DIR "" CACHE PATH "Directory containing joltc.h")
set(JOLTC_LIBRARY "" CACHE FILEPATH "joltc shared library, or its import library on Windows")

if (NOT JOLTC_INCLUDE_DIR OR NOT JOLTC_LIBRARY)
    message(FATAL_ERROR "Set JOLTC_INCLUDE_DIR and JOLTC_LIBRARY")
endif()

add_executable(determinism_check determinism_check.c)
target_include_directories(determinism_check PRIVATE ${JOLTC_INCLUDE_DIR})
target_link_libraries(determinism_check PRIVATE ${JOLTC_LIBRARY})

if (MSVC)
    target_compile_options(determinism_check PRIVATE /O2 /fp:precise)
else()
    target_compile_options(determinism_check PRIVATE -O2 -ffp-contract=off)
    target_link_libraries(determinism_check PRIVATE m)
endif()
