using UnityEngine;
using Random = UnityEngine.Random;

public class Crystal : MonoBehaviour
{
    [SerializeField] private float rotateSpeed;

    private float _xRand;
    private float _yRand;
    private float _zRand;

    private void Awake()
    {
        InvokeRepeating(nameof(Roll), 0.0f, 0.2f);
    }

    private void Update()
    {
        transform.Rotate(Vector3.right, rotateSpeed * Time.deltaTime * _xRand);
        transform.Rotate(Vector3.up, rotateSpeed * Time.deltaTime * _yRand);
        transform.Rotate(Vector3.forward, rotateSpeed * Time.deltaTime * _zRand);
    }

    private void Roll()
    {
        _xRand = Random.Range(-0.5f, 1.0f);
        _yRand = Random.Range(-0.5f, 1.0f);
        _zRand = Random.Range(-0.5f, 0.5f);
    }

    public void Accelerate()
    {
        rotateSpeed += 20.0f;
    }

    public void Decelerate()
    {
        rotateSpeed -= 20.0f;
    }
}
