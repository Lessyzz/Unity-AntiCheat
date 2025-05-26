# Script Features

## Speed Hack Prevention:
Protects against cheats that artificially modify the game's speed. It operates without affecting game performance or FPS. (Speed hacks set to 1.1 using Cheat Engine are detected within approximately 2 seconds. Highly sensitive and optimized.)

## Memory Manipulation Protection:
Detects and blocks attempts to manipulate memory using custom defined data types. Variables behave like standard Int, Float, etc., but encryption and decryption happen transparently in the background. If memory manipulation is detected, the game automatically closes. Additionally, each variable has a randomly generated encryption key.

### Supported Types: Integer, Float, String, Bool, Long, DateTime.

## Popular Modding Tools Blocking:
Automatically blocks several known modding and cheat tools.

## Performance Friendly:
Ensures security without disrupting gameplay flow or player experience.

# Usage:
Simply create an empty GameObject in your scene and attach the script.
