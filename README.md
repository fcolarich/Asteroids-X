AsteroidsX

ASTEROIDS-X – By Francisco Colarich

The project consists on developing a version of the Atari’s Asteroids game using DOTS.

The game is a 2D shooter where the player controls a spaceship that has to avoid being hit by asteroids while destroying them with its gun. The level is wrapped around the borders, so anything crossing one side, appears on the other instantly (including bullets and the player ship).
The asteroids come in 3 sizes, big, medium and small. Each one, except the small, when destroyed produce 2 smaller asteroids that fly faster than their predecessor and in random directions. The small asteroids are the only ones that when destroyed leave the level entirely.

Each asteroid gives points when destroyed, 20 points for the big one, 50 points for the medium one and 100 points for the small ones.
There are also 2 types of UFOs, a small one that pursues the player trying to destroy it and a big one that shoots in random directions. The big ship gives the player 200 points and the small one 1000 points. The UFOs appear randomly, but they tend to appear when there are less than 30% asteroids left.
When getting 10.000 points, the player earns a life.

The player can only accelerate in the direction the spaceship is looking, but can change the rotation of the ship at any moment. 
The player can only shoot 3 bullets in short succession before waiting for a small time to “reload”.
The player can do an Hyperspace jump that transports instantly the ship from one place to a random site in the level, with the risk of appearing inside an asteroid and losing a life.
The player starts the game in the middle of the screen and with 3 lives.
The first wave of asteroids consists of 4 big ones. Each new wave adds 2 big asteroids to the previous amount. A wave ends when the player destroys all asteroids on screen.

So far, this are the basics of the game as it was developed originally.

To this, we must add power ups: one (or more) shot variations and a temporary shield.
…

To this, I have decided to add certain modifications to make the game more fun and dynamic, as well as add more challenge and variation to the gameplay.
First of all, the game will be set on the StarWars universe. 
The original movies and the Atari game came with barely 2 years of difference, and everything about the game makes me remember those times. So, it seemed appropriate to combine them.

The players will be pilots of a pair of X-Wings (hence the name Asteroids-X) and they will have to navigate an enemy ridden asteroid field. Obviously, we know they won’t make it alive, but the player can help them kill as many Empire ships as possible.

There will be several power ups, including the two required:
-	Shields (grant invulnerability)
-	Big Blasters (bullets are bigger, increasing the chances of hitting something)
-	Super Blasters (shots go through everything)
-	Engine Booster (faster movement)
-	Fire Rate Booster (faster shooting)
The UFOS will behave more aggressively:
-	The small UFO will keep following the player trying to ram it
-	The medium UFO will shoot directly at the player
-	There will be big UFO that will be some kind of Boss, that will shoot at the player and spawn smaller UFOs when hit. This will be the only entity in the game with hit points. This enemy will appear every fixed number of rounds to test the players.

Basic System Layout
The basic system layout is as follows:
The translation of all the entities is managed by one system, that reads the MovementSpeed (which is used as a proxy for velocity, but without physics) and applies a translation given the delta time.

Asteroids are spawned and initialized with one movement value, calculated at random taking into account their maximum speed. Then, they are moved by the general translation system. Bullets spawned by players and enemies alike follow a similar pattern.

Most enemies will keep a constant speed and change only their rotation to attack the player, either by firing at them or trying to ram them.
The player entities have also acceleration, friction and rotation data components that allow the respective systems to calculate the final speed and heading of the players.
There is one catch all trigger detection system, having configured the collision layers, it only detects the collisions needed and adds a component when required, as well as sending information between the entities, for example, to add points to the right player when an enemy is destroyed or to apply a power up effect to the right player. Then, each type of entity (player, enemy or powerups) has a collision reaction system, that processes their data accordingly and changes it and other entities accordingly.

The entities with most data associated are the players, given they not only need to listen to player input, but to store the lives, points, current weapon, firerate, etc.
There is also a Wave Manager system that controls what happens each wave, who gets spawned and where. 

Finally, we have the ScreenWrapping system, that allows the screen to wrap depending on its dimensions, teleporting any entity that goes over one side to the other, just like in the classic.

To support this, we have a traditional UI, particles and audio, that get controlled either by events, by UI actions or managed hybrid systems. Particles and audio are pooled to increase performance.

..
Final thoughts:
Although I achieved what I set myself to achieve, I still wish I could have completed the bonus level, which was going to be a 3D shooter, with a view from behind the cockpit, where the player had to destroy asteroids and enemies that were coming towards them, like old school flight games.
Also missing and already scheduled to be implemented are the store with permanent upgrades module, the leaderboard, as well as some variety in the game modes.
