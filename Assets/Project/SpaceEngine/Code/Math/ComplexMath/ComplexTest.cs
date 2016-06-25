using UnityEngine;

public class ComplexTest : MonoBehaviour
{
    public Complex a = new Complex(0, 100);
    public Complex b = new Complex(1, 50);
    public ComplexVector v = ComplexVector.Zero;

    private void Start()
    {
        Debug.Log("A: " + a.ToString());
        Debug.Log("B: " + b.ToString());
        Debug.Log("A + B: " + (a + b).ToString());

        Debug.Log("V: " + v.ToString());
    }
}