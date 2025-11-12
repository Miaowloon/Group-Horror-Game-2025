using UnityEngine;
using UnityEngine.AI;

public class FleeAI : MonoBehaviour
{
    // --- Flee Settings ---
    public float fleeDistance = 10f; // How close the player must be to trigger a flee
    public float fleeRange = 20f;   // How far away the NPC should try to run

    // --- Peek Animation Settings ---
    [Header("Peek Settings")]
    public float minPeekInterval = 5f; // Minimum seconds between peeks
    public float maxPeekInterval = 10f; // Maximum seconds between peeks

    private Transform player;
    private NavMeshAgent agent;
    private Animator animator; 
    private float peekTimer; // Timer for the periodic peek animation

    void Start()
    {
        // Find the player object using its tag ("Player")
        GameObject playerObject = GameObject.FindWithTag("Player");
        if (playerObject != null)
        {
            player = playerObject.transform;
        }
        else
        {
            Debug.LogError("Player object with the 'Player' tag not found! NPC cannot flee.");
        }
        
        // Get required components
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        
        if (animator == null)
        {
            Debug.LogError("Animator component not found on this GameObject.");
        }
        
        // Initialize the peek timer to a random value
        peekTimer = Random.Range(minPeekInterval, maxPeekInterval);
    }

    void Update()
    {
        if (player == null || agent == null || animator == null) return;

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        // --- Flee Logic ---
        // Check if the player is within the initial trigger distance
        if (distanceToPlayer < fleeDistance)
        {
            // Only calculate a new flee spot if the agent has reached its last destination 
            // or is stopped. This prevents constant recalculation (running in circles).
            if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
            {
                Flee();
            }
        }
        else
        {
            // If the player is outside the flee distance, ensure the agent stops moving 
            // once it reaches its current destination.
            if (agent.hasPath && agent.remainingDistance <= agent.stoppingDistance)
            {
                 agent.SetDestination(transform.position);
            }
        }
        
        // --- Animation Logic ---
        UpdateAnimation();
        UpdatePeekTimer();
    }

    void Flee()
    {
        // 1. Calculate the direction *away* from the player
        Vector3 directionToPlayer = transform.position - player.position;

        // 2. Calculate a position far away in that direction
        Vector3 newDestination = transform.position + directionToPlayer.normalized * fleeRange;

        // 3. Find the closest valid point on the NavMesh
        NavMeshHit hit;
        if (NavMesh.SamplePosition(newDestination, out hit, fleeRange, NavMesh.AllAreas))
        {
            // 4. Set the NavMeshAgent's destination
            agent.SetDestination(hit.position);
        }
    }
    
    // Links the movement speed to the Animator component
    void UpdateAnimation()
    {
        // Get the magnitude (speed) of the agent's velocity.
        float speed = agent.velocity.magnitude; 
        
        // Pass the calculated speed to the Animator's "Speed" float parameter
        animator.SetFloat("Speed", speed);
    }

    // Handles the timing for the "peek" animation
    void UpdatePeekTimer()
    {
        // Check if the NPC is actively running (using a small speed threshold)
        if (agent.velocity.magnitude > 0.1f) 
        {
            peekTimer -= Time.deltaTime;

            if (peekTimer <= 0f)
            {
                // Fire the Animator Trigger to start the peek animation
                animator.SetTrigger("StartPeek");

                // Reset the timer to a new random interval
                peekTimer = Random.Range(minPeekInterval, maxPeekInterval);
            }
        }
        else
        {
            // If the NPC is idle, reset the timer so it counts down properly when running starts
            peekTimer = Random.Range(minPeekInterval, maxPeekInterval);
        }
    }
}