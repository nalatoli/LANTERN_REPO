/* Essential Namespaces */
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/***************************************************************************************** 
 * The 'PlayerMovement' class controls the movement of the player. The player has basic 
 * movement options such as walking, jumping, and climbing. Furthemore, to generate more
 * realistic motions, horizontal movement is velocity-based, as opposed to position-based.
 * Vertical movement is handled by the RigidBody2D component of the player
 * 
 * Units used:
 *  u - unit (width of generic 3D cube object)
 *  s - second
 *  
 ****************************************************************************************/

public class PlayerMovement : MonoBehaviour {

    /* Enumeration of Player State */
    public enum PlayerState {
        grounded,   // Player is on the ground
        airborne,   // Player is in the air
        crouching   // Player is crouching
    }

    /* Parameters */
    public PlayerState currentPlayerState;  // Holds state of player 
    public float maxSpeed;                  // Maximum speed of player [u/s]
    public float timeToMaxSpeed;            // Time it takes for player to reach max speed [s]
    public float timeToMinSpeed;            // Time it takes for player to reach 0 [u/s]
    public float maxJumpHeight;             // Maximum jump height of player [u]
    public float crawlSpeed;                // Speed of player while crawling [u/s]
            
    /* Private Variables */
    private Rigidbody2D rb;         // RigidBody2D component of player
    private float currentVelocity;  // X-Velocity of player [u/s]


    /****************************************************************************************************************************************************************************************************
     * Function: Start
     * Inputs: N/A
     * Outputs: N/A
     * Description: Initializes all private variables
     * 
     */

    void Start () 
    {
        /* Initialize Private Variables */
        rb = GetComponent<Rigidbody2D>();   // Get RigidBody2D component
        currentVelocity = 0;                // Initialize 'currentVelocity'
	}

    /****************************************************************************************************************************************************************************************************
     * Function: Update
     * Inputs: N/A
     * Outputs: N/A
     * 
     * Description: Generates appropriate player movement in response to user input. The default control 
     * scheme is "WASD". 'Crouch Mode' is entered upon pressing 'S' when not airborne. When crouching, the
     * player's box collider is modified and crawling is enabled. Crawling further modifies the box collider, 
     * reduces the max speed of the player, and disables player acceleration.
     *  
     *  Key |  Grounded  |  Airborne  |  Crouching
     * -----|------------|------------|-------------
     *   W  |    Jump    |   (N/A)    |   (N/A)
     *   A  | Walk Left  | Move Left  | Crawl Left
     *   S  |   Crouch   |   (N/A)    |  Stand Up
     *   D  | Walk Right | Move Right | Crawl Right
     *   
     */

    void Update () 
    {
        /* Choose function depending on state. Each function modifies the horizontal velocity of the player */
        switch (currentPlayerState) {
            case PlayerState.grounded:  // If player is on the ground,
                GroundMove();           // Call 'GroundMove()'
                break;

            case PlayerState.airborne:  // If player is in the air,
                AirborneMove();         // Call 'AirMove()'
                break;

            case PlayerState.crouching: // If player is crouching,
                CrouchMove();           // Call 'CrouchMove()'
                break;
        }

        /* Move player. The new position of the player is based of the new 'currentVelocity' parameter. */
        Move(currentVelocity * Time.deltaTime); 
    }

    /****************************************************************************************************************************************************************************************************
     * Function: GroundMove
     * Inputs: N/A
     * Outputs: N/A
     * Description: Calls approriate 'grounded-based' functions depending on keypress. 
     *  NOTE: 'd' key has priority over 'a' key
     * 
     */

    void GroundMove ()
    {
        /* Call appropriate vertical-movement function for grounded player */
        if (Input.GetKeyDown("w")) {                    // If 'w' is pressed,
            ForceUp();                                  //  Make player jump and
            currentPlayerState = PlayerState.airborne;  //  mark the player as airborne
        }

        if (Input.GetKeyDown("s")) {                    // If 's' is pressed,
            currentPlayerState = PlayerState.crouching; //  Mark player as crouching
        }

        /* Call appropriate horizontal-movement function for grounded player */
        if (Input.GetKey("d")) {        // If 'a' is pressed,        
            SpeedUp(Vector2.right);     //  Add negative velocity to player
        }

        else if (Input.GetKey("a")) {   // Else, if 'd' is pressed,
            SpeedUp(Vector2.left);      //  Add positive velocity to player
        }

        else {                          // Else, 
            SlowDown();                 //  Decrease magnitude of player's velocity
        }
        
    }

    /****************************************************************************************************************************************************************************************************
     * Function: AirborneMove
     * Inputs: N/A
     * Outputs: N/A
     * Description: Calls approriate 'airborne-based' function depending on keypress
     *  NOTE: 'd' key has priority over 'a' key
     * 
     */

    void AirborneMove ()
    {
        /* Call appropriate horizontal-movement function for airborne player */
        if (Input.GetKey("d")) {        // If 'a' is pressed,
            SpeedUp(Vector2.right);     //  Add negative velocity to player
        }

        else if (Input.GetKey("a")) {   // Else, if 'd' is pressed,
            SpeedUp(Vector2.left);      //  Add positive velocity to player
        }

        else {                          // Else, 
            SlowDown();                 //  Decrease magnitude of player's velocity
        }

        /* Determine if player is still airborne */
        if (rb.velocity.y == 0) {                       // If the player is NOT moving in the y-direction,
            currentPlayerState = PlayerState.grounded;  //  Mark the player as grounded
        }
    }

    /**************************************************************************************************************************************************************************************************** 
     * Function: CrouchMove
     * Inputs: N/A
     * Outputs: N/A
     * Description: Calls approriate 'crouch-based' function depending on keypress
     *  NOTE: 'a' key has priority over 'd' key. Furthermore, velocity is no longer to
     *  move the player. Acceleration and deceleration is disabled. Instead, the player
     *  is simply translated based on 'crawlSpeed'
     *   
     */

    void CrouchMove () 
    {
        /* Decrease velocity of player. Ensures that velocity of player is eventually zero. */
        SlowDown();

        /* Call appropriate vertical-movement function for crouching player */
        if (Input.GetKeyDown("s")) {                    // If 's' is pressed,
            currentPlayerState = PlayerState.grounded;  //  Mark the player as grounded
        }

        if (Input.GetKeyDown("w")) {                    // If 'w' is pressed, 
            ForceUp();                                  //  Make player jump and
            currentPlayerState = PlayerState.airborne;  //  Mark the player as airborne
        }

        /* Call appropriate horizontal-movement function for crouching player */
        if (Input.GetKey("d")) {                // If 'a' is pressed,
            Move(crawlSpeed * Time.deltaTime);  //  Move player to the right
        }

        else if (Input.GetKey("a")) {           // Else, if 'd' is pressed,
            Move(-crawlSpeed * Time.deltaTime); //  Move player to the left
        }
    }

    /**************************************************************************************************************************************************************************************************** 
     * Function: ForceUp
     * Inputs: N/A
     * Outputs: N/A
     * Description: Adds +y-velocity to player so player reaches APPROXIMATELY 'maxJumpHeight' units as max 
     *  height.
     *  NOTE: The speed at which the player reaches 'maxJumpHeight' can be modified by changing the
     *  gravity scale of the player.
     *  The instant velocity 'v' is calculated as:
     *  
     *  v(g,s,h) = sqrt(-2 * g * s * h)
     *  
     *  g: acceleration due to gravity [u/s^2]
     *  s: gravity scale of player     [UNITLESS]
     *  h: maxJumpHeight               [u]
     * 
     */

    void ForceUp () 
    {
        /* Assign instantaneous +y-velocity */
        rb.velocity = new Vector2(0, Mathf.Sqrt(-2.0f * Physics.gravity.y * rb.gravityScale * maxJumpHeight));
    }

    /* 
     * Function: SpeedUp
     * Inputs: dir
     * Outputs: N/A
     * Description: Adds x-velocity in 'dir'. The function only modifies the velocity if max speed is not reached.
     *  NOTE: The acceleration used is dependent on if the magnitude of the player's velocity is increasing or 
     *  decreasing.
     *  
     *  The infinitesimal velocity change 'dv' is calculated as:
     *  
     *  dv(d,a,dt) = a * dt
     *  
     *  a: signed acceleration of player (max speed / time to max/min speed) [u/s^2]
     *  dt: time elapsed between frames                                      [s]
     * 
     */

    void SpeedUp (Vector2 dir) 
    {
        /* Perform velocity adjustment based on current velocity and 'dir' */
        if (dir == Vector2.left && currentVelocity > -maxSpeed)                                                         // If 'dir' is left and max speed in -x direction is not reached,
            currentVelocity -=  maxSpeed * Time.deltaTime / (currentVelocity < 0 ? timeToMaxSpeed : timeToMinSpeed);    //  Subtract dv from current velocity. Uses acceleration if
                                                                                                                        //  the current velocity is negatve, otherwise, uses deceleration.

        else if (dir == Vector2.right && currentVelocity < maxSpeed)                                                    // Else, if 'dir' is right and max speed in +x direction is not reached,
            currentVelocity += maxSpeed * Time.deltaTime / (currentVelocity > 0 ? timeToMaxSpeed : timeToMinSpeed);     //  Add dv to current velocity. Uses acceleration if
                                                                                                                        //  the current velocity is positive, otherwise, uses deceleration.
    }

    /****************************************************************************************************************************************************************************************************
     * Function: SlowDown
     * Inputs: N/A
     * Outputs: N/A
     * Description: Lowers magnitude of x-velocity.
     *  NOTE: The magnitude of x-velocity is lowered until it reaches a threshold defined as deceleration / 10.
     *  
     *  The infinitesimal velocity change 'dv' is calculated as:
     *  
     *  dv(d,a,dt) = (+/-) (|v| - a * dt)
     *  
     *  (+/-): Sign of current velocity     [UNITLESS]
     *  |v|: Magnitude of current velocty   [u/s]
     *  a: deceleration of player           [u/s^2]
     *  dt: time elapsed between frames     [s]
     * 
     */

    void SlowDown ()
    {
        /* Perform velocity adjustment */
        if (currentVelocity != 0) {                                                                                                     // If the current velocity of the player is NOT zero,
            currentVelocity = Mathf.Sign(currentVelocity) * (Mathf.Abs(currentVelocity) - maxSpeed * Time.deltaTime / timeToMinSpeed);  //  Decrease the magnitude of the current velocity

            if (currentVelocity < maxSpeed / timeToMinSpeed / 10 && currentVelocity > -maxSpeed / timeToMinSpeed / 10)                  //  If current velocity is between threshold,
                currentVelocity = 0;                                                                                                    //   Set current velocity to zero

        }
    }

    /**************************************************************************************************************************************************************************************************** 
     * Function: Move
     * Inputs: distance
     * Outputs: N/A
     * Description: Translates player by 'distance' in the x-direction.
     *
     */

    void Move (float distance)
    {
        /* Translate Player */
        transform.position = new Vector2(transform.position.x + distance, transform.position.y);
    }
}
