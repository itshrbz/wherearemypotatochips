using UnityEngine;

public class SandboxPatrol : MonoBehaviour
{
    public float speed = 3f, distance = 12f;
    Vector3 a, b; bool toB = true;
    void Start() { a = transform.position; b = a + transform.forward * distance; }
    void Update()
    {
        Vector3 goal = toB ? b : a;
        transform.position = Vector3.MoveTowards(transform.position, goal, speed * Time.deltaTime);
        Vector3 d = goal - transform.position; d.y = 0;
        if (d.sqrMagnitude > 0.01f)
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(d), 8f * Time.deltaTime);
        else toB = !toB;
    }
}