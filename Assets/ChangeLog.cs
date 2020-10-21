/*##### 0.3.4 #####
 * --- Bugs ---
 * Made turn text always on top
 * Fixed enemy repeating first action error
 * Fixed forced movement distance error
 * Fixed broken intent system showing incorrect intents
 * 
 * --- Cards ---
 * Nerfed Arrow cast range from 5 -> 3
 * 
 * --- System ---
 * Randomly destroy a small number of rooms in overworld to add variation
 * 
 * --- UI ---
 * Moved card on click magnify up more to account for fat thumbs
 * 
 * ##### 0.3.5 #####
 * --- Gameplay ---
 * Added bonus attack and shields tied to movement for enemies and players
 * Rebalanced shields and attacks for all units to adjust
 * 
 * ##### 0.3.6 #####
 * --- Gameplay ---
 * Removed bonus attack and shields tied to movement for enemies and players
 * Player can no longer attack without card
 *  * Player can no longer move after action
 * 
 * --- Cards ---
 * Added Attack cards for each player
 * Cards now use energy and mana
 *     * Using Energy generates Mana, 1 Energy used -> 1 Mana generated
 *     * Energy autogenerates everyturn, Mana stays constant till used
 *     
 *     --- Added ---
 *     Barrier
 *     - Blue
 *     - 1 energy
 *     - 0 mana
 *     - Raise a target's shield by 6
 *     
 *     --- Changed ---
 *     -- Blue --
 *       Haste
 *       - 1 -> 0 energy
 *       - 0 -> 1 mana
 *       - Grant +1 move range for 2 -> 3 turns
 *       Marked for Death
 *       - 1 -> 0 energy
 *       - 0 -> 2 mana
 *       - +1 Emfeble for 1 -> 3 turns
 *     -- Green --
 *       Arrow
 *       - 1 -> 0 energy
 *       - 0 -> 2 mana
 *       Bear Trap
 *       - 1 -> 0 energy
 *       - 0 -> 1 mana
 *       Boomerang
 *       - 1 -> 0 energy
 *       - 0 -> 1 mana
 *       - Echo effect moved to OnPlay condition instead of OnCast
 *     -- Red --
 *       Get Over Here
 *       - 1 -> 0 energy
 *       - 0 -> 1 mana
 *       Taunt
 *       - 1 -> 0 energy
 *       - 0 mana
 *     
 * 
 * --- System ---
 * Added replace card option, available twice per turn
 * Added hold card option, keep 1 card till next turn
 * Mana renamed to Energy
 * Added new Mana system
 * Cards are added to a queue executed at the end of turn instead of immediately
 * Now able to cancel attacks in queue
 * Added OnPlay conditions for cards
 * 
 * --- UI ---
 * Attack sequence now shown on top of screen, will execute one at a time
 * Rearranged play space to accomodate area on top of screen
 * Added area for replacing cards
 * Added area for holding cards
 * 
 * --- Bugs ---
 * Removed additional collider size from bottom of attack queue UI cards
 * Removed Grey Attack and Grey Defend cards from loot table
 * 
 * ##### 0.3.7 #####
 * 
 * --- Cards ---
 *     --- Added ---
 *     -- Red --
 *     Bring It On
 *     - 0 energy
 *     - 1 mana
 *     - Taunts all enemies in 2 range for 1 turn
 *     
 *     --- Changed ---
 *     -- Blue --
 *     Shatter Shield
 *     - 1 -> 0 energy
 *     - 0 -> 1 mana
 *     - Lose 2 armor, deal 2 piercing damage to all enemies in range -> Lose all armor, deal that much damange damage to the target
 *     -- Red --
 *     Get Over Here
 *     - Pull a target towards you -> Pull a target towards you and attack for 80% of Attack
 *     
 * --- Enemies ---
 * Added 10% random variation in health of enemies (prevent a swarm of 9 health situation)
 * Added Aggro variants of all enemy sizes
 * Added more 5 more room variants
 * 
 * --- UI ---
 * Added different cardbacks for Skill and Attack cards
 * Cards attack damage now dynamically change to reflect caster ATK
 * 
 * --- System ---
 * pulled camera back a bit
 * Added GetCurrentShieldEffect()
 * Added Barrier to Rewards loot table
 * 
 * --- Bugs ---
 * Fixed energy not reseting after ending encounter with less than full energy
 * Fixed cancelling of echo cards
 * Chaged Card outlines to reflect the skill/attack card difference
 * Fixed new enemies not having the enemy tag
 * Fixed new enemy cards not showing the correct intent
 * 
 * ##### 0.3.8 #####
 * 
 * --- Cards ---
 *     --- Changed ---
 *     -- Red --
 *     Massive Swing
 *     - 2 -> 0 energy
 *     - 0 -> 2 Mana
 *     
 *     -- Green --
 *     - Cast shape from circle to plus
 *     - Attack with 100% ATK -> Attack with 100% ATK and Knockback 1
 * 
 * --- System ---
 * Added ability to modify deck at the beginning of the game and between each encounter
 * Added ability to show card numbers without modifying the attack numbers with the character's ATK
 * Added allowing knockback to overlap enemies
 *      Attacks hit all enemies in the same cell
 * 
 * --- Bugs ---
 * Fixed dynamic ATK numbers crashing reward cards
 * Fixed resolve overlap not resetting objects back into the grid
 * Fixed blocks sometimes overlapping with eachother on spawn
 * 
 * #####################
 * ####### 0.3.9 #######
 * #####################
 * 
 * --- Cards ---
 *     --- Added ---
 *     -- Blue --
 *     Smite
 *     - 0 energy
 *     - 2 mana
 *     - Deal 66% of ATK as damage to all enemies within 2 spaces
 *     
 * --- UI ---
 * Unplayable cards are now greyed out
 * Added buff icons to players and enemies showing the buff and the duration left
 * Added character detail screen on click
 * 
 * --- System ---
 * Added TargetedAoE cast type to cards
 * Added metrics tracking to the backend
 * Reverted attack preview
 * Reverted unable to move after attack
 * Added dynamic damage numbers to enemy cards
 * 
 * --- Bugs ---
 * -- Cards --
 *      Fixed AoE cast type for enemy cards
 *      Fixed grow not giving strength
 *      Fixed int float calculation error for Vit and Piercing damage
 * Fixed not checking death on enemy turn
 * Fixed rewards cards being greyed out
 * *** Fixed infinite loop error? maybe? ***
 * 
 * #####################
 * ####### 0.4.0 #######
 * #####################
 * --- Cards ---
 *     --- Changed ---
 *     -- Red --
 *     Overdrive
 *     - Deal 150% of ATK as damage, lose 2 health --> Deal 150% of ATK as Damage, lose 50% of ATK as health
 *     Taunt
 *     - Force an enemy to target you for 2 turns --> Force an enemy to target you for 2 turns and gain 2 armor
 *     
 *     -- Blue --
 *     Haste
 *     - 1 -> 0 mana
 *     
 *     -- Green --
 *     Explosive trap
 *     - Set a trap that deals 3 piercing damage --> Set a trap that deals 250% of ATK as piercing damage
 *     Arrow
 *     - Deal 100% of ATK as Damage, twice --> Deal 50% of ATK as damage four times
 *     
 * --- Enemies ---
 * Added swarm boss
 *      Summons 4 10/0/8 enemies
 *      Buff own attack based on number of enemies around
 *      Attack
 * 
 * --- UI ---
 * Added Card Art for all current cards
 * Added Player sprites and shadows
 * Added Enemy sprites and shadows
 * 
 * --- System ---
 * Added * to dynamic damage numbers if the caster has bonus attack
 * Added ability to avoid objects of certain tags when checking for pathfinding
 * Added pathfinding, spawning, and path previewing for enemies of 2x2 size
 * Fixed multilple triggers when card was played on overlapping characters in grid
 * Fixed enemy cards not having a caster
 * Fixed card selection null exception
 * Fixed randomize room pathfinding error
 *      Also allowed initial pathfinding to avoid player and enemies in collision checking
 * Always goes to the deck manager screen after each combat
 * Deck manager now highlights new cards
 * Added data tracking for rewards cards
 * Added data tracking for collection cards
 * Added data tracking for hold and replace cards during combat
 * Enemies will now ignore 0 health players
 * Enemies will try to move away from their starting positions before attacking if possible
 * Rewards at the end of encounter will never give a card that the player has at least 4 copies of
 * Card texts now reflect attack bonuses after getting an attack buff
 * Added healthbars that only appear on hit
 * No longer logs information when in debug mode
 * Only objects with size 1 can be force moved
 * Healthbars that overlap is now scaled to the biggest enemy in the pile
 * Damage is reduced by shield and shield decreases by 1 (but never less than 1), when shield reaches 0 the character is broken and takes double damage
 * 
 * --- Bugs ---
 * Fixed initial card text not reflecting player attack
 * Fixed GetObjectsInAoE taking in 1 space too little
 * Fixed Card not returning to original position when cast location is not viable
 * Fixed roomsetups not being loaded before going into a room
 * Fixed bomb casting on other enemies in range
 * Fixed not updating lastgoodposition after player not being forcemoved
 * Fixed traps not triggering
 * Fixed enemy AoE cast
 * Enemy on click information now shows AoE ranges
 * Fixed enemies not going to the furthest location they can
 * Fixed enemies not being able to attack if attack range is 1 and target is within but not at the edge of range
 * Clearified text on Rallying Cry and Overdrive
 * Fixed bugs with enemy AoE casts triggering multiple times
 * Fixed error with size 2 enemies casting AoE
 *      
 * #####################
 * ####### 0.4.1 #######
 * #####################
 * Added initial set for playtesting
 * 
 * --- Cards ---
 *      -- Red --
 *          CRE01 - Knockback
 *          - Get Over Here
 *          - 1/0
 *          - Pull a target from up to 2 spaces away towards you and deal 80% of ATK as damange
 *          CRE02 - Damage - Erange
 *          - Shield Slam
 *          - 1/0
 *          - Lose half your armor, deal that much damage to a target
 *          CRE03 - Damage
 *          - Overdrive
 *          - 0/0
 *          - Deal 150% of ATK as damage, deal 50% of ATK as damage to self
 *          CRE04 - Shields
 *          - Taunt
 *          - 
 *      -- Blue --
 *      -- Green --
 * 
 * --- Enemies ---
 * Will now awlays check to see if there's any target it can attack
 * If the desired target is out of range, it will attack a target closest to the desired target from a postion closest to the desired target's location
 * Enemies will now try to get the most number of targets in the AoE if casting an AoE card
 *      -- Crystalizer --
 *          Crystalize gains 3 armor instead of 5
 *          Now randomly does 1 or 2 crystalize before attacking
 * 
 * 
 * --- UI ---
 * Pulled camera back slightly in the overworld
 * Adjusted combat space a bit to give cards more room
 * Added Immune to status texts
 * AoE enemy cards now show effect radius during cast
 * Added version number to main menu
 * Added different tiers of attack intent images
 * Enemy intent images now refreshes on damage
 * Enemy intent now show a multiplier if specified in the card
 * Added ability to speedup all timers in the game
 * Added anticipated mana gain and loss when using cards
 * Added more opacity to tiles
 * Added shield damage image on shield damage
 * 
 * --- System ---
 * Added device ID to the information logged
 * Enemies will now be allowed to spawn 1 unit lower
 * At room randimization, it will attempt the grid layout with the assigned number of blocks twice. If failed, try whole process again with 1 less block
 * Will no longer allow players to be clicked while in Enemy Turn and will no longer allow the moving enemy to be clicked while moving
 * Moved target selection into CardEffectController for both player and enemy cards
 * Enemies cards will now respect card execution priority, negative first, positive last. Default 0
 *      Ties will be resolved by moving enemies closest to their target first
 * ReImplimented bonus shields
 * Effect process is now a coroutine to allow for delays and animations
 * Add slight pulse in hit damage number between multiple attacks
 * Ensure that card for enemy casts NEVER overlaps the target
 * Implimented card based target behaviour that overrides enemy's target behaviour for finner control. Default to enemy behaviour unless specified
 * Added a centralized timer handling system
 * 
 * --- Bugs ---
 * Fixed grey out on enemy display cards
 * Enemy intents will dissapear even if they didn't attack
 * Fixed bug where player start of turn effects are triggered twice per turn
 * Fixed cards being unable to uncast
 * Fixed targeting lowest vit targeting lowest shield instead
 * healing will no longer show the - sign
 * Fixed enemies multicasting on self
 * Fixed AoE cast counting blockades
 * Fixed room spawning error where blocks were never removed from the grid
 * Fixed gust not being + cast
 * Fixed pathfinding not attacking if desired target was out of range
 * Fixed being able to move 1 tile after casting cards
 * 
 * -- To dos --
 * melee range attack can't find target doesn't try to hit closer enemy if unpathable
 * 
 * #####################
 * ####### 0.4.2 #######
 * #####################
 * -- Cards --
 * Reimplimented basic set with defensive cards
 * Refrased Protect card's description
 * Refrased permanent and temporary armor
 * Retuned card values for balance
 * 
 * -- Enemies --
 * Reimplimented all enemies
 * 
 * -- UI --
 * Added colored outline for bonus armor
 * Added background to turn counter
 * Added darken effect to decks not being selected in deck manager
 * Added yellow highlights for castable targets
 * Enemy Intents now also display the color of the target they have chosen
 * Enemy Intents will grey out slightly if the intended target is out of range
 * Remove intents on dead characters
 * Sorted overlapping enemies based on health (dead sorted to bottom, then lowest health first, players always on top)
 * Now allows multiple cards per enemy to be displayed on hold and on character information page
 * Cards can now have dynamic text sizing on long card names
 * Added ability texts to the character information menu
 * Added flashing to unit sprite when the unit has an ability
 * Clicking characters how shows their healthbars
 * Added Overheal component to health bars
 * Added flicker to gaining mana
 * Added Move Trail for all player characters
 * Taking 0 damage now also shows damage numbers
 * Bonus shield buffs now show bonus shield numbers
 * Darkened cards in the deck collection section are no longer changeable, now must navigate to that color's tab to change that color's cards
 * Added UI showing number of moveranges remaining while adjusting to move
 * 
 * -- System --
 * Armor will now only reset to starting armor after being broken if armor is lower than starting armor
 * Added previous effect successful condition
 * Reworked cards to use cardcontrollers instead
 * Enemy cards will always be cast on the taunted player even if it's an enemy cast type
 * Card effects are now coroutines and will wait for the previous to finish before starting the next one
 *      Forced movement bypasses this and will move at the same time as the previous effect
 * Enemies are now able to be use more than 1 attack card per turn
 * Added ability system to all characters
 *      Added OnSacrifice effects
 *      Added OnDeaht effects to all characters
 * Added overheal mechanic: excess healing is retained as bonus vit till the start of the next turn
 * It is now impossible to get a room twice in a row
 * Enemies now become unbroken at the start of player turns, not start of enemy turns (much more consistent for "if broken" cards)
 * Enemies no longer attack any target other than the intended one
 * First 3 rooms are now much easier, last 2 rooms are old difficulty, middle rooms are slightly easier
 * 
 * -- Bugs --
 * Fixed Drawing an extra hand if a card was held from last turn
 * Piercing no longer amplified by enfeeble
 * Fixed tempvalues and energy/mana reduction sticking after the end of the encounter
 * Fixed energy cost debuff applying incorrectly
 * Fixed temporary armor not blocking broken status
 * Fixed temporary armor not being reduced while broken
 * Fixed text formating on new line characters
 * Fixed forced movement cards not damaging as well (forced movement must be the last effect in sequence)
 * Fixed forced movement ignoring colision check on caster's location
 * Fixed Healthbar on heal showing incorrect amounts (white section too high)
 * Fixed (R) Bolster being an AoE cast instead of targeted
 * Fixed mana discounts reducing ALL cards of the same name's costs
 * Fixed Rewards cards being broken after cardcontroller rework
 * Fixed Card cost reduction drawn discounting the leftmost card even if previous drawing effect wasn't able to draw a card
 * Fixed held card playability not being reset
 * Fixed Bomb enemy causing a null reference error on it's target call
 * Fixed fast short drags of characters bring up the character information menu
 * Fixed enemy attack card never triggering the second card
 * Fixed enemies showing intent when below 0 health
 * Fixed character showing broken text when there is still bonus armor
 * Fixed character information menu showing extra cards
 * Fixed error where draw specific card effects can't draw any cards
 * Fixed mana numbers not reflecting cost reduction
 * Fixed enemy cards erroneously resorting to self instead of default
 * Fixed self target cards showing intents where the target is out of range
 * Fixed layering issues with selected card names
 * Fixed boss room collider being too small
 * Fixed enemy intent colors being called before blocks are being spawned
 * Fixed UI card title text still show even when behind selected card on hover
 * Fixed enemies with 2+ attacks per turn never changing their second attack
 * Fixed draw mana cards and draw energy cards doing incorrect null checks with monobehaviours whose gameobject has been destroyed
 * Fixed BossRoom collider not being disabled causing objects in the top middle of the board to be unselectable
 * Fixed taunts not taking effect the turn it is played
 * Fixed summoned enemies displaying intent
 * Fixed crash at the start of the turn when taunted player has died in the previous turn
 * Fixed AOE cards being used up when cast was unseccessful
 * Fixed linerenderer lingering for duration of the effect process after cast
 * Fixed yellow selectable tiles were behind the enemy highlights
 * Fixed yellow selectable tiles show up on blockades
 * Fixed intent images not being updated when the enemy has more than 1 card it can use
 * Fixed plus shaped tile ranges would go out of bounds
 * Fixed intent multiplier didn't hide with intent
 * Fixed card will return to hand briefly while it's effect is being processed
 * Fixed boss room went into a normal room instead
 * Fixed sprite appearing before the stat circles and blocking text
 * Fixed Massive Stomp not showing the correct intent image
 * Fixed intent being greyed out when it should be at the beginning of every turn (character transforms are off by 0.007104, may want to check down the road)
 * Fixed vitdamage doing 100% of attack as damage in cards that have the potential to do 0 damage due to temporaryValue
 * Fixed incorrect showing of the broken status when there is bonus shield
 * 
 * #####################
 * ####### 0.4.3 #######
 * #####################
 * -- Features --
 * Implimented all 6 colors' starting set common cards
 * Can now choose between all 6 colors in the tavern
 * Added Gold system. Passive and overkill
 * Added Shops
 * Added Relics
 * Added score tracking
 * Added save/load
 * Added a lives system
 *      If a player dies, their cards become a raise card that resurrect the player if there are lives left
 *      If out of lives their cards are unplayable
 *      *Players stay dead and no longer rez at 1 health if killed in combat. Dead players will stay dead
 * 
 * -- Cards --
 * 
 * -- Enemies --
 * Support now grants +1 moverange instead of +1 armor
 * Nerfed enemy survivability across the board
 * 
 * -- System --
 * Added passive gold for completing a combat room
 * Added 1 gold per overkill health
 * Added Shops: enough gold to buy 2 cards with passive gold, 3 cards if actively going for overkill gold
 * Added information logs to shops and gold
 * Added the relic system
 * Randomly grants 1 relic at the starting of the game
 * Added the ability for roomsetups to allow 1 random relic pickup in rewards at the end of the room
 * Added Shrines: Choose between a permanent stat increase or a relic
 * Added a score tracking system
 * Added a seed system to the game's randomization
 * Added a save load system that saves:
 *      Seed
 *      Overworld layout and progress
 *      Selected and collected cards
 *      New cards
 *      Relics
 *      Shrine stats bonuses
 *      All score controller score values
 *      Colors of the players in the game
 * Added ability to choose the other 3 colors
 * Added much more robust and futureproof sorting to card effects, condition types, and buff times
 * Added cast range controlls to all characters
 * Added tavern to allow players to choose their party
 * Added save and load system for player preferences (party setup and settings)
 * Check death now triggers after buffs
 * Added trigger ticket system to buff triggers so removing of buffs only happens after the last buff has been triggered
 *     Avoids issues where an end of turn buff triggers and deletes itself, but an on damage buff tries to access the modified list
 * 
 * -- UI --
 * Added to rewards screen gold gain for clearing combat, gold gain from overkill
 * Added relic rewards to rewards menus, including descriptions on chosen.
 * Added UI for when a relic triggers
 * Added endgame score page
 * Added damage number to card displays when it's not dynamic
 * Adjusted many UI elements to adjust according to screen width
 * Held card is no longer click or dragable during enemy turn
 * Changed formatting code handeling for dynamic card text to be much more robust to adding new formatting codes
 * Flipped card art for mana cards, and added better mana/energy cost display
 * Changed energy icons to red in card and in combat UI
 * Recentered and improved card text positioning and wrapping
 * Added conditional highlight options for cards to indicate they will use the alternate effect or their effect will trigger with no problem
 * Buffs now trigger one at a time on each characters one at a time for greater clarity
 * Added hover card enlarge to rewards cards, shop cards, and deck customize cards
 * 
 * -- Bugs --
 * Fixed relics causing infinite loops on trigger
 * Fixed Shop and Reward cards from being affected by mana changing effects during combat
 * Fixed combat rooms not remembering setup on load
 * Fixed layer ordering issues
 * Fixed 0 number of energy cards played relics always triggering
 * Fixed card display crashing when the card text had multiple '%' characters in it
 * Fixed boss spawns not dying if killed after the boss
 * Fixed tiles not being created for player movement if enemy was knocked onto the original position
 * Fixed certain buff times not triggering due to not being called as coroutines
 * Fixed player stats not saving if they died in the first room
 * 
 * --------------------------------------------------------------------------------------------------

 * -- To dos --
 * Simplify common cards
 * 
 * #####################
 * ####### 0.4.4 #######
 * #####################
 * -- Cards --
 * 
 * -- Enemies --
 * 
 * -- System --
 * 
 * -- UI --
 * 
 * -- Bugs --
 * 
 * Add permanent progression per character and per team
 *  Unlocks tallents for each member (chose 1 tallent tree)
 *  Unlocks uncommon cards for the char
 *  Unlocks uncommon relics
 *  Unlocks team based progression (
 *  Unlocks characters for the party
 * 
 * Add world 2
 *  
 * Add options menu
 *      animation speeds
 * Add UI for key words explanation of cards
 * 
 * Add encyclopedia of cards that've been encountered in the game
 * 
 * move buffs to ability controllers? or seperate buffs to follow the listener pattern
 * 
 * Add starting relic bonus to be only if last run ended above level 5, otherwise get a tiny stat boost instead (1 armor or 3 health)
 * Have a bigger visual effect for recovering from broken
 * 
 * Retune Armro/Attack/Health on all characters including players (armor for how many turns till broken, then adjust attack so ~30%-50% of attack goes through on full armor, health based on how many rooms till healing on average)
 *      retune energy defence cards strength so 1 defence blocks most of attack, 2 blocks all from normal enemies, 3 blocks all from bosses too
 *      retune mana defence cards strength os 1 blocks all from normal enemies, 2 blocks all from bosses including special attacks, 3 to block ALL possible damage
 * 
 * Add more support for playing lots of cards per turn in uncommon
 * Add water/pits
 * Add terrain like traps and fires and events (falling rocks)
 * Think about preventing long term stalling
 * Add more bosses
 * Add elite minibosses (have hearthstone solo adventure style passive effects)
 *      Huge stats, duplicate (split stats equally across the two spawns), attack, duplicate, attack
 * tune all enemy values
 * 
 * #####################
 * ####### 0.4.5 #######
 * #####################
 * -- Cards --
 * 
 * -- Enemies --
 * 
 * -- System --
 * 
 * -- UI --
 * 
 * -- Bugs --
 * 
 * Uncommon: powerful but more complex, requires more setup to get the most out of them
 * Add uncommon cards
 *      Add Mana Overflowing. 2 energy gain 2 additional mana
 *      Rupture: target looses 1 armor per each space knockbacked this turn
 * 
 * Add uncommon relics
 *      relic for doubling passive gold and moving overkill gold to 0
 *      relic for doubling overkill gold and moving passive gold to 0
 * 
 * add control cards to basic set
 * 
 * #####################
 * ####### 0.4.6 #######
 * #####################
 * -- Cards --
 * 
 * -- Enemies --
 * 
 * -- System --
 * 
 * -- UI --
 * 
 * -- Bugs --
 * 
 * solidify common cards, add uncommon cards
 * 
 * Add challenge modes: 10 health, etc
 * 
 * Add permanent scoring save. Score translates to team levels at the end of each run
 *      unlock uncommon cards
 *      unlock more common relics
 *      unlock more uncommon relics
 *      unlock new challenge mode options
 *      unlock currency for alternative starting attack and defence cards
 *      unlock new character after first boss beat run
 *      unlock second character after first boss beat run with 1 challenge mode
 *      unlock third character after first boss beating the run under X time
 *      unlock 1 of 2 customize card slot on every character
 *      cards that have been in the collection in a run are displayed in a cardopedia
 *          Cards that have been in the final deck when beating the game are golden
 *      give the player the option to unlock 1 card in their cardopedia to use in the customize card slot
 *          commons are cheap, uncommons are expensive, legendaries are INSANELY expensive/impossible? (potential monitization option)
 *          gold varieties have a discount
 * Add achievements
 *      Take no damage in a boss encounter
 *      Take no damage in a run
 *      Finish the game with 1 challenge enabled
 *      Finish the game with 3 challenge enabled
 *      Finish the game with 5 challenge enabled
 *      Finish the game with all challenges enabled
 *      Play 10 cards in a single turn
 *      Fill the mana bar
 *      Use 15 mana in a turn
 *      Win an encounter with all energy cards
 *      Win an encounter with all mana cards
 *      Deal damage to 4 enemies with a single card
 *      Let an enemy kill another enemy
 *      End the turn with more than 20 armor
 *      End the turn with more than 30 health
 *      End the turn in every square
 *      Move one character 13 squares in 1 turn
 *      Use an attack card while the caster has moer than 15 attack
 *      Don't recieve ANY damage for an entire encounter
 *      Deal 50 damage to a single enemy with a single card
 *      Avoid 100 damage in a single turn
 *      Overkill an enemy by 100
 *      End the turn with 4+ enemies broken
 *      End an encounter without leaving a single energy unused every turn
 *      Use 20 mana in a single encounter
 *      He attacc, he protecc, but most importantly he thicc as hecc
 *      Move an enemy 10 blocks in a single turn
 *      Finish the game after spending 999 gold
 *      Finish the game without equiping a single mana card
 *      Get 10 golden cards for Red, yellow, gree, etc
 *      Get 20 golden cards for red, yellow, green, etc
 *      Get 30, 40, 50, etc
 *      
 *      Tiered slots, only allow uncommon, commoon, rare, etc
 *      unlock a slot after getting all cards
 *      
 *      unlock tiers of opening relics
 *      
 *      golden, would give a shit, unlock gold cosmetic for character when all their cards are golden
 *      
 *      card cosmetics, (cast line change)
 *      
 *      daily hero contracts(color tied)
 *      daily quests (do x damage)
 *      
 *      permanent story progression tied to expansions
 * 
 * #####################
 * ####### 0.4.7 #######
 * #####################
 * -- Cards --
 * 
 * -- Enemies --
 * 
 * -- System --
 * 
 * -- UI --
 * 
 * -- Bugs --
 * 
 * add in 6 rares (1 color per archytype)
 * 
 * #####################
 * ####### 0.4.8 #######
 * #####################
 * -- Cards --
 * 
 * -- Enemies --
 * 
 * -- System --
 * 
 * -- UI --
 * 
 * -- Bugs --
 * 
 * add armor/accessories
 * 
 * #####################
 * ####### 0.4.9 #######
 * #####################
 * -- Cards --
 * 
 * -- Enemies --
 * 
 * -- System --
 * 
 * -- UI --
 * 
 * -- Bugs --
 * 
 * add weapons
 * 
 * #####################
 * ####### 0.5.0 #######
 * #####################
 * -- Cards --
 * 
 * -- Enemies --
 * 
 * -- System --
 * 
 * -- UI --
 * 
 * -- Bugs --
 * 
 * First alpha build
 * Get feedback
 * Add cross game loops
 *      Permanent upgrade system (secondary currency that persists between games)
 *          Give player the feeling that if the current run is hopeless, I can go after permanent resources to upgrade my next run
 *      Upgrades: <Incentivise next run planning>
 *          Able to carry 1 card that's been unlocked (picked in a pervious game) to the start of the next game (of your choice)
 *          Able to carry 1 card from the deck from the last run to the next game
 *          Unlock a starting pack (very low currency requirement, get it essentially the second run)
 *          Upgrade rarity of the starting pack (uncommon, rare, epic, legendary, etc)
 *          Unlock blessings/curses (+1 to all attack, but -1 move range, etc)
 *          Unlock revives (bring back to 1 health at the cost of one of the three most used cards)
 *      Hub unlocks: <All manned by quirky characters> <Incentivise next run planning>
 *          Library (All the cards in collection in previous runs) <Tied to upgrades?>
 *          Armory (All weapons and armor in collection in previous runs) <Tied to upgrades?>
 *          Bank (Bonus gold at the begining of runs) <Gamble in daily challenges>
 *          Training Hall (Achievements and challenges) <No mana cards above 3, but gain +50% gold or all elites have +50% ATK but gives 2 rare cards>
 *                  Unlock new characters here
 *      Achievements: <Incentivise long term run planning across many runs>
 *          Dealt 100 damage with one card
 *          Obtained all common cards, uncommon cards, etc
 *          Obtain all blue cards, red, etc
 *          Magic Tower: Win a run with red, orange, and white
 *          Slash and Burn: Win a run with red, green, and blue
 */