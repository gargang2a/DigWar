using UnityEngine;

namespace Game
{
    [RequireComponent(typeof(Collider2D))]
    public class Rock : MonoBehaviour
    {
        // Future: Add durability or explosion effect
        private void OnCollisionEnter2D(Collision2D collision)
        {
            // Logic for hitting a rock (e.g. bounce, screen shake)
        }
    }
}
