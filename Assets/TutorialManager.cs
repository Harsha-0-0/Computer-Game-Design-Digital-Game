using UnityEngine;

public class TutorialManager : MonoBehaviour
{
    [Header("Key Hints")]
    public GameObject keyHintMovement;
    public GameObject keyHintJump;

    [Header("Instruction Signs")]
    public GameObject sign1;
    public GameObject sign2;
    public GameObject sign3;
    public GameObject sign4;

    [Header("Mug Reference")]
    public Transform mug;

    [Header("Stage Trigger Positions")]
    public float stage2TriggerX = 8f;
    public float stage3TriggerX = 18f;
    public float stage4TriggerX = 28f;

    [Header("Sign Hide Positions")]
    public float hideSign1X = 6f;
    public float hideSign2X = 16f;
    public float hideSign3X = 26f;

    private int currentStage = 1;
    private bool sign1Hidden = false;
    private bool sign2Hidden = false;
    private bool sign3Hidden = false;

    void Start()
    {
        if (sign1 != null) sign1.SetActive(true);
        if (sign2 != null) sign2.SetActive(true);
        if (sign3 != null) sign3.SetActive(true);
        if (sign4 != null) sign4.SetActive(true);

        // Show movement hint immediately
        if (keyHintMovement != null) 
            keyHintMovement.SetActive(true);
        
        // Hide jump hint until stage 2
        if (keyHintJump != null) 
            keyHintJump.SetActive(false);
    }

    void Update()
    {
        if (mug == null) return;

        float mugX = mug.position.x;

        // Show jump hint when approaching platform 2
        if (mugX >= 2f && keyHintJump != null && 
            !keyHintJump.activeSelf)
        {
            keyHintJump.SetActive(true);
        }

        // Hide movement hint
        if (!sign1Hidden && mugX >= hideSign1X)
        {
            sign1Hidden = true;
            if (sign1 != null) sign1.SetActive(false);
            if (keyHintMovement != null) 
                keyHintMovement.SetActive(false);
        }

        // Hide jump hint
        if (!sign2Hidden && mugX >= hideSign2X)
        {
            sign2Hidden = true;
            if (sign2 != null) sign2.SetActive(false);
            if (keyHintJump != null) 
                keyHintJump.SetActive(false);
        }

        // Hide collect hint
        if (!sign3Hidden && mugX >= hideSign3X)
        {
            sign3Hidden = true;
            if (sign3 != null) sign3.SetActive(false);
        }
    }
}