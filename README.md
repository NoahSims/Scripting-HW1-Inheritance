Scripting-HW1-Inheritance

For this project, we were given the player controller and the green tank asset and instructed to build a boss fight around it, giving the boss at least 2 attacks and at least 2 movement patterns. I modified the player controller slightly to allow for 8 directional shooting.\
My boss has 3 methods of attacking the player, which are cycled through using a state machine.
- Default: random charge attack - the boss charges in a random direction around the arena, dealing contact damage to the player, and switching directions on hitting the wall.
- On taking damage, 50% chance to trigger: Circle & Ram attack - the boss strafes around the player in a circle, then stops displays a telegraph before charging directly at the player, becoming stunned on hitting the wall.
- On falling below half health: Pillar & Mortar attack - The boss moves to the top of the arena and is raised above the player on a pillar. The boss then shoots at the player from above and the player must destroy the pillar to be able to damage the boss again.

Gameplay Footage

https://user-images.githubusercontent.com/56094247/139747595-3b29f6f9-f6cb-435f-b048-8c7fe63f53f6.mp4
