﻿/*##### 0.3.4 #####
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
 * Added bonus attack and armors tied to movement for enemies and players
 * Rebalanced armors and attacks for all units to adjust
 * 
 * ##### 0.3.6 #####
 * --- Gameplay ---
 * Removed bonus attack and armors tied to movement for enemies and players
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
 *     - Raise a target's armor by 6
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
 *     Shatter Armor
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
 * Added GetCurrentArmorEffect()
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
 * Damage is reduced by armor and armor decreases by 1 (but never less than 1), when armor reaches 0 the character is broken and takes double damage
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
 *          - Armor Slam
 *          - 1/0
 *          - Lose half your armor, deal that much damage to a target
 *          CRE03 - Damage
 *          - Overdrive
 *          - 0/0
 *          - Deal 150% of ATK as damage, deal 50% of ATK as damage to self
 *          CRE04 - Armors
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
 * Added armor damage image on armor damage
 * 
 * --- System ---
 * Added device ID to the information logged
 * Enemies will now be allowed to spawn 1 unit lower
 * At room randimization, it will attempt the grid layout with the assigned number of blocks twice. If failed, try whole process again with 1 less block
 * Will no longer allow players to be clicked while in Enemy Turn and will no longer allow the moving enemy to be clicked while moving
 * Moved target selection into CardEffectController for both player and enemy cards
 * Enemies cards will now respect card execution priority, negative first, positive last. Default 0
 *      Ties will be resolved by moving enemies closest to their target first
 * ReImplimented bonus armors
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
 * Fixed targeting lowest vit targeting lowest armor instead
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
 * Bonus armor buffs now show bonus armor numbers
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
 * Fixed incorrect showing of the broken status when there is bonus armor
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
 *      Overworld
 *      layout and progress
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
 * #####################
 * ####### 0.4.4 #######
 * #####################
 * -- Features --
 * Added World 2
 * 
 * -- Heroes --
 * - Blue -
 * Moverange from 4 -> 3
 * 
 * - Black -
 * Castragne from 2 -> 3
 * 
 * - Yellow -
 * Moverange from 3 -> 4
 * 
 * - Green -
 * Armor from 6 -> 3
 * 
 * -- Cards --
 * All 90 starting and common rarity cards implimented
 * 
 * -- Enemies --
 * 
 * -- System --
 * Officially changed all "armor" references in code to "armor". Card names can still contains "armor"
 * Buff trigger chains will now no longer trigger buffs already triggered in the chain (prevents infinite buff trigger chains)
 * Buffs apply/trigger/revert information are now tracked for analysis in combat logs
 * Game score at the end of the game is now tracked for analysis
 *      A blank score line is saved at the begining of every game to track if that run has been abandoned
 * Implemented taunt on players
 * Implemented stun on players
 * Implemented silence and disarm on enemies
 * Enemy cards now use enemycontroller cast range instead of card castrange
 * If enemy is AoE casting with a single player as the desired target, it will now move towards that desired player instead of maximizing number of ANY players in the AoE
 * Now saves high score in each category as well as highest total score
 * Added team and hero progression system
 * Added currency rewards
 * Added lives rewards
 * Added replace rewards
 * Added tavern contract rewards
 * Added tavern hero unlocks
 * New hand system: at the end of the turn, all unplayed cards are held, temporary cards are removed, starting hand size increased to 6
 * Total EXP is stored for potential future patching
 * Added Manifest effect: Chose 1 of 3 cards to add to your hand
 * CardDisplay is now consistently used as a prefab in all card prefabs, to simplify all future CardDisplay changes
 * Added phased movement to players and enemies
 * OnCardPlayed now triggers for all players, not just the player who casted the card
 * Added possibility of applying buffs on forced movement collision
 * Draw cards at start of each turn now happens before all start of turn buffs
 * Minimum Castrange for all characters are capped at 1
 * Added settings menu and saving settings for game speed and screen shake amount
 * Reset levels in tavern now resets the battle pass as well as eraces current save
 * Room layout, room setup, rewards cards, relic rewards, card draw/shuffle, shop offerings and shrine rewards are now all predetermined by the initial seed (seed setting and loading game)
 * Games is now saved while entering and exiting every room. Once entered, restarting only allows the entered room to be entered (prevent save scumming)
 * 
 * -- UI --
 * Recentered all character displays
 * Added idle animations to all characters (player and enemy)
 * Added casting animations to all players
 * Added attackig animations to all players
 * Added death animations to all players
 * Added death animations to all characters
 * Added running, attacking, and death to enemies
 * All characters will not be destroyed after the death animation instead of immediately
 * Enemies now move more smoothly
 *      Will face the direction they're attacking/moving
 *      Will return to face right at the end of their turn
 * Timer for the game is now displayed on screen
 * Cards now have a wiggle and point when dragged and while casting
 * All cards types during combat have been updated for the perspective camera
 * Card processes are now tied to attack animation
 * Added blood splatter to on damage effects
 * Added on hit particle effect to all cards
 * Added camera shake for all events in combat
 * Added camera shake if card is targeting a castable location
 * Added red overlay for when players take damage. Intensity tied to remaining health of the player
 * Added disabled status text on cards UI for silenced, disarmed, and stunned
 * Drawing more cards in hand will now no longer overflow outside screen space
 * Sped up attacking animation for all player characters
 * Fixed none type cast cards
 * Added shatter effect to shield UI on break
 * Endscreen now shows past high scores
 * Added tracking and saving of individual color hero levels and team levels
 * Beating the boss now heals and resurrects all players for free
 * Added more fanfair to beating the boss
 * Added card draw animation
 * Added effect animation for characters recovering from break
 * Added effect for drawing and destroying of temporary cards
 * Added manifest effect for ANY energy cards
 * Added targetable locations highlight for TargetedAoE cast types
 * Added slight delay in movement per block for multi space forced movement
 * Rewards and shop cards are now dynamic text unless held to enlarge
 * Added Battlepass UI for team and hero unlocks in the tavern screen
 * Boss screenshake now overrides setting for no screenshake
 * Added options to enable remaining moverange indicator
 * Character health bars are now higher up to avoid the finger on mobile
 * Manifest card choices now have a "Choose 1" text and a button for hiding the cards to see the board
 * 
 * -- Bugs --
 * Fixed turn based duration buffs not reducing in duration if they arent start or end of turn trigger buffs
 * Fixed bug that would cause resurrected players to be rezed at the wrong location
 * Fixed attack text not being shown in character information screen
 * Fixed card isTargetingCaster not looking at the current target
 * Fixed card copies not copying the hiteffect
 * Fixed damage recieved buff triggers not passing the attacker along
 * Fixed temporary armor never allowing the armor to be reduced
 * Fixed buffs sometimes double reverting
 * On player hit overlay now only triggers when the card dealt damage instead of when any card targeted a player (buffs)
 * Fixed bug where enemies would keep running animation past their turn
 * Fixed cost display for selected cards not showing properly
 * Fixed seed and version next not refreshing and staying after main menu scene upon new game after completing the last one
 * Fixed buffs not triggering at the right time for the right duration
 * Fixed death game end continue button not working
 * Fixed error where clicking a specific part of the relic obtain button would load the game into the first room of the second world
 * Fixed party energy and mana reduction being caps instead
 * Fixed crash on second world room
 * Fixed saving always being 1 room behind
 * Fixed debug enemy being in a room world 2
 * Fixed cards still showing when all characters are defeated
 * Fixed G Power Shot not working
 * Fixed World 2 Damage enemy destroying itself in the idle animation
 * Fixed BossSummonSupport healing the boss on death
 * Fixed Relic/gold/lives UI not showing after load
 * Fixed Disabled cards (stunned/silenced/disarmed) still showing the text after no longer being disabled
 * Fixed rewards controllers sometimes going to rooms due to roomController overlap. RoomController is now moved out of the way in combatScenes
 * Sacrifice and CreateObject effects now ignore taunt
 * Fixed world 2 room spawn being too far to the right of where it's supposed to be
 * Players can no longer be moved while a card is casting (avoid position and forcemove errors)
 * Fixed copy cards not copying buffs or highlight conditions
 * Fixed healing showing blood splatter
 * Fixed names of various cards turning into white blocks
 * Fixed drawing cards removing the card then drawing the card, now draws then removes
 * Fixed steal card effect not drawing the card the turn it's used
 * Fixed crash when enemy tries to target a dead player
 * Fixed on card played buffs being triggered by enemy cards
 * Fixed player getting double lives during level up
 * Fixed relic unclickable after seeing the enemy information screen (due to card disabled status text being way too big)
 * Fixed unlocked lives not being loaded
 * Fixed crash when an enemy another enemy is targeting dies before the enemy turn starts
 * Can't cast on self cards can now actually not be cast on self
 * Fixed cleanse effects not making disabled cards immediately playable
 * 
 * #####################
 * ####### 0.4.5 #######
 * #####################
 * -- Cards --
 * All red cards now give permanent armor instead of temporary armor
 * 
 *      - Red -
 *      Headcrack --> Shield Slam
 *          If the target is attacking you, stun them for 1 turn --> Deal damage equal to your armor
 *      
 *      - Blue -
 *      Blood Rite
 *          Whenever the target is healed, resture 3 more health --> Restore 150% of ATK as health three times
 *      Life Link --> Holy Smite
 *          Whenerver the target is healed, you heal for the same amount --> Targeted AoE, Heal the center target for 150% of ATK, deal 150% of ATK as damage to surrounding targets
 *      Cycle of Mana
 *          Draw 2 cards --> Expand Hand Size by 1
 *      
 *      - White -
 *      Echo
 *          Echoed card now keeps all cost reductions and caps of the played card
 *      Rememberance
 *          2M --> 1E
 *          Manifest a card from your discard pile --> Manifest a white temporary copy of a card from your discard pile
 *      Drain Strength --> Foresight
 *          2E --> 0E
 *          Target looses 2 ATK, you gain 2 aTK --> Manifest a a white temporary copy of a card from your draw pile
 *          
 *      - Black -
 *      Soul Catcher --> Poison
 *          Remove 3 armor from the target, they gain 1 back over the next 3 turns --> for the next 3 turns, remove 1 armor and deal 25% of ATK as damage
 *      Belittle --> Alchemic Ferver
 *          Target looses 1 ATK for each status effect on them --> For the next 3 turns, targets restores 1 health for each energy used
 * 
 * -- Enemies --
 * Lowered health of all World 2 normal enemies by 25%
 * 
 * -- System --
 * Added Multiplayer
 * Added World 3
 * Buff will now no longer trigger buffs of the same trigger type. (bleeds will not trigger a different bleed)
 * Added traps and associated enemy movement
 *      Enemies will try to avoid traps if possible
 * Added gravity and knockback targeting for enemies
 *      Enemies will always prioritize stacking players first. If that's not possible, knocking players into traps
 * When two traps are created on top of eachother, the longer duration one stays
 * Added name and art for passives
 * Enemy processes will not wait for it's effect to finish triggering before moving on to the next enemy
 * Knockback effects from enemies will now always prioritize stacking players
 * Enemies can now use targeted AoE on other enemies
 * Doubled shrine possibility on all worlds
 * Room randomization of world layers is no longer hard coded
 * Lowered number of rooms in each world by 1
 * Card effects that don't impact the board no longer pauses for 0.5 seconds
 * Traps duration change is now independent of damage trigger and no longer delays end of turn
 * Added mana and energy use as buff trigger types
 * Added data tracking to playtime information
 * Added data tarcking to playtime inside combat (tracked every player and enemy turn)
 * 
 * -- UI --
 * Added tooltips to keywords in enemy AND player cards when cards have been selected
 * Added cards at start of combat, start of turn, and in character information screen for passive effects
 * Added passive card backs
 * Added glow hit particle for passive effects
 * Added health damage previews when casting cards (direct and buff. Will have to update with ability and talent in the future)
 * Added health damage previews for end of turn damage over time
 * 
 * -- Bugs --
 * Fixed traps not being spawn under characters in AoE cast
 * Fixed traps spawning on top of eachother in AoE cast
 * Fixed red linerenderer after opening character information menu on enemies with passives
 * Fixed passive casts not destroying the target tile reticle
 * Fixed world 3 enemies animations not looping
 * Fixed bug where holding on enemies showed passive card instead of next attack card on turn 1
 * Fixed bug where stored information always shows missing for room name
 * Fixed bug where on damage dealt buffs were triggered during health bar simulations
 * Fixed multiple simulation targets messing up each other's values by adding in a pooling system of 49 simulation targets
 * Fixed bug where knockback effects could knock enemies into blockades
 * Fixed bug where simulation objects will be picked up in player object finds
 * Fixed preview healthbar not showing up for Holy Smite
 * Fixed timelogging not saving the combat room name, only combat room type
 * 
 * 
 * #####################
 * ####### 0.4.6 #######
 * #####################
 * -- Cards --
 * 
 * -- Enemies --
 * 
 * -- System --
 * Added story mode
 * Added achievement system and basic achievements
 * Roomsetups can now specify room layout that is deterministic or remain truely random
 * Added item system that keeps track of item inventory
 * Added tracking to if challenge items have been sold
 * Added tracking for all item inventory across singeplayer games
 * Added tracking of bought cards (daily/weekly special or otherwise) across singeplayer games
 * Added system and UI for selecting weapons and accessories for different colors
 * Non basic cards can no longer be slotted into basic card slots
 * Added save/load to story mode's cards and equipments
 * Cards can now trigger equipment effects. Equipment effects always trigger before the card's own effects goes off
 * Complete and selected decks now reworked to be consistent for all 6 colors instead of just the 3 in party
 * Cards and Equipments can now be given as rewards at the end of story mode rooms
 * Add weapons to shop pool
 * 
 * -- Story --
 * Added flavor text for first 10 rooms in world 1
 * 
 * -- UI --
 * Added room icons to all story mode rooms
 * Added different color for 3 stars achieved and 3 challenge rewards bought
 * Added unlocked material list to story mode shop screen
 * Added materials icon to story mode end shop screen
 * Added achievement progress for unsatisfied achievements in final shop screen
 * Added items and stars tabs to main story mode screen
 * Added new card crafting screen that shows required and obtained materials
 * Reworked deck manager to account for weapons and equipments
 * Added way to navigate between Map, Party, Gear, Card, and Skill tabs in story mode
 * Added UI selection for gear when selecting, deselecting, and swapping equipment with already equiped gear
 * Added weapon/accessory/basic card equiping/unequiping/sorting that is independent of each other
 * Added equipment changing removing non starter cards who's slots are no longer available
 *       Ensured this works for equipment from other colors who swapped with this color's equipment too
 * Added UI conversion between card display and selectable cards in the collection controller
 * Added conditional UI whiteout highlights for equipment and card selection
 * Added menu selection UI to story mode
 * Added warning icon for card button if their cards no longer satisfy a full deck's requirement, disable other buttons till that's sorted
 * Basic starter cards are now always sorted first in the collection manager
 * Added icons and tooltips to card displays for weapons with on play effects
 * Player equipped equipments can now be seen on the character information menu
 * Added Equipment, Card, and Material type to story mode end scene
 * 
 * -- Bugs --
 * Fixed room not showing yellow even if achievements are fully done for the room
 * Fixed achievements being overwritten by future, worse attempts
 * Fixed room and deck resetting incorrectly after completing a room
 * Fixed end of non end maps not reporting end of turn achievements
 * Fixed bug where path preview UI will path through enemies and blockades
 * Fixed combat scenes camera not positioned correctly
 * Fixed the shop back button not actually returning to the main menu
 * Fixed achievement tracking for the first 5 rooms
 * 
 * -- To dos --
 * ~ Add card drag deselect for equipment cards
 * ~ Make selectable cards able to be dragged from one type (basic slots) to another (weapon slots)
 * ~ Add on hover card display to card and weapon in story mode end scene and item section of story mode scene
 * Design enemy, achievement, and reward for the first world
 * Add mini boss for the first world (when broken/summons destroyed, grant player three 0 cost synergizing cards)
 * Add secret shop (use stars for unlocking permanent bonuses (2 cards per room, etc)
 * Add secret area (just a tonne of bombs)
 * Add arena, essentially classic mode for the 1 world (bonuses for short time and low number of cards used. 5-10 tiers, better reward each time)
 *      Add prepped (can bring in any equpment and card) and naked (yes equipment, but no card) versions
 *      Best equipment and cards can only be unlocked this way
 * 
 * Design maps and enemy designs for all 10 rooms
 * Design item reward strucutre for all 10 rooms
 * Add shop and crafting (1 normal shop, probably connected to room 2 or 3, 1 secret shop, revealed when you 3 star room 5 or 6 or 7)
 *      Shop (gold as currency)
 *          1 customizeable slot
 *          1 weapon slot
 *          1 equipment slot
 *          Daily card specials (50% off materials)
 *              1 Common Card
 *          Weekly card specials (25% off materials)
 *              3 Common Cards
 *          Crafting for all common cards that have been seen
 *          1 net cost equipments
 *              Sprint shoes
 *                  +1 moverange
 *              
 *      Secret Shop (stars as currency)
 *          1 customizeable slot
 *          + passive gold
 *          2 cards per room reward
 *          Card rewards reroll
 *          Unlock uncommon cards
 *          Materials trading (lower tiered to higher tiered materials)
 *          Restat weapons
 *          2 net cost weapons
 *              Midas Gauntlet
 *                  Deal +50% damage to enemies below 0 health
 *          Daily and Weekly specials on uncommon cards
 *          
 * Add weapons (relics for story mode. Effect with negative stats randomly generated)
 *      Cost value associated with effect, balanced by stats
 *              1 cost = 1 ATK
 *              1 cost = 2 Armor
 *              1 cost = 5 Health
 *      Cost can be -1 cost in a random stat or +1 cost in a stat and -2 in another or +1 in a stat and -1 in two others
 *      ie: break mana effect = 1 cost, so stats can be -1 ATK, or +1 ATK and -10 Health or +2 Armor but -1 ATK and -5 health
 *      Or Equipments: +1 ATk or +2 ATK and -5 health, etc
 *      Rarer weapons have 2 or 3 cost effects and can have wider cost ranges
 *      Rarer equipments have 2 or 3 cost stats and can have wider cost ranges
 *      Weapons stats versions are randomly generated so many weapons with the same effect can exist (name of weapon tied to effect, not stats)
 *          Secret shop can restat weapons?
 *      Weapons have stats and add 1 special card to your deck
 *          Grey in preview, Uncommon level complexity, powerful if drawn
 *          Build around or payoff cards for a color's archetype
 *      Equipments are relics from classic mode
 *      Switch so weapon is stats and equipment is effect?
 *      Eventually each char can equip 1 weapon and 2 equipment
 * Add arenas (1 no customizable card slot unlocked, 1 completely unlocked. Battle till death, mini version of the classic gameplay)
 * Secret rooms
 *      1 or 2 per world that's an interesting take on a room/enemy combination
 *      Not nessecarily hard, but a cool interesting reward for those who 3 star everything
 *      Places for mini games?
 *      Bomb voyage: smaller and smaller rooms with more and more bombs
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
 * --------------------------------------------------------------------------------------------------
 * -- To dos --
 * Simplify common cards
 * Add permanent progression per character and per team
 *  Unlocks tallents for each member (chose 1 tallent tree)
 *  Unlocks uncommon cards for the char
 *  Unlocks uncommon relics
 * 
 *  
 * Add options menu
 *      animation speeds
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
 * Add water/pits
 * Add terrain like traps and fires and events (falling rocks)
 * Think about preventing long term stalling
 * Add more bosses
 * Add elite minibosses (have hearthstone solo adventure style passive effects)
 *      Huge stats, duplicate (split stats equally across the two spawns), attack, duplicate, attack
 * tune all enemy values
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