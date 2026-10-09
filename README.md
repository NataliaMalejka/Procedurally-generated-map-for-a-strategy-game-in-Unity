# Procedurally-generated-map-for-a-strategy-game-in-Unity

A procedural world generation system for a turn-based strategy game inspired by Civilization VI and Humankind.<br>
The project focuses on generating diverse, replayable hex-based worlds with interconnected terrain, climate, and biome systems.

Key Features:

- Customizable World Generation - Players can choose the map size, select between 1 and 3 map levels, and optionally provide a seed to reproduce a specific world.<br>
- Procedural Terrain Generation - Creates continents with varied elevation using multi-octave noise, along with mountain ranges and rivers that shape the landscape.<br>
- Climate Simulation - Calculates temperature based on distance from the equator and humidity based on wind direction, terrain elevation, and distance from the ocean.<br>
- Biome Generation - Determines biomes from temperature and humidity data, creating diverse environmental regions across the map.<br>
- Biome-Specific Structures - Randomly distributes unique structures according to the biome in which they appear.<br>
- Unit Movement - Allows players to navigate the generated world using a selected unit.<br>

Technologies:<br>
C# · Unity · Procedural Generation · Multi-Octave Noise · Hexagonal Grid
