using UnityEngine;

public class JumpAnchor : MonoBehaviour
{
    public RhythmLeaf thisLeaf;
    
    private void Start() {
        thisLeaf = GetComponent<RhythmLeaf>();
    }
}
