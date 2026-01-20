using UnityEngine;

public class USpinMeRoundRound : MonoBehaviour
{
    public Vector3 rotationSpeed = new Vector3(0, 100, 0); // градусы в секунду

    void Update()
    {
        transform.Rotate(rotationSpeed * Time.deltaTime);
    }
}
