using Unity.VisualScripting;
using UnityEngine;

public class CarMove : MonoBehaviour
{
    [SerializeField] private int speed;

    void OnEnable()
    {
        speed = Random.Range(12, 17);
    }

    private void Update()
    {
        transform.position += transform.forward * speed * Time.deltaTime;
    }

    private void OnTriggerEnter(Collider other) 
    {
        transform.position = new Vector3(-15f, transform.position.y, transform.position.z);
    }
}
