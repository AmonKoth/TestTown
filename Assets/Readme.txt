This project is created while studing in CEA Game developement program. I've tried to keep everything in that courses scope.

Created a 3rd person character controller which can just jump right now but later it will be able to attack and die.
The animations apply root motion, meaning the movement speed is tied to the animation speed.
The character models and animations are from Adobe Mixamo.
Unity's Cinemachine, InputSystem, NavMeshAgent and UI libraries are used. 
added-Will add death animation and health system after adding the AI
Seperated UI and PlayerController components into 2 different components in order to not cause confusion. Later, will do this to other components as well.

created the ai using what we learned in the scope of the class.
created "states" for the AI and AI makes decisions in the boundry of this states(like wander, follow player etc);

If needed(depending on your feedback) can add more user feedback like weapon hit particles or ai death particles etc.

Movement Keys - WASD
Camera Follows Player's head (Cinemachine)
Jump Space - standing still while jumping charges the jump
Mouse will make player look in the scene
DELETED => For now i've also added a standing still rotation key (q,e) however will delete those later
Q - Equips Weapon
Mouse 1 - Fires the weapon(has sublte firing animation, also have laser sight for better aiming)
ESC- opens the pause menu

Houses and Powerups are made using proBuilder in unity

Imported Terrain textures from UnityTechnologies

Gun is a free asset from : https://assetstore.unity.com/packages/3d/props/guns/sci-fi-gun-light-87916
