using System;
using UnityEngine;
using Pebble;
//----------------------------------------------
// CardComponent
//----------------------------------------------
// Purpose:
//   Visual/interactive representation of a `BeloteCard` in the scene.
//   Handles selection/drag based on mouse input and notifies gameplay
//   via `BeloteCard.Selected` events.
//
// How it connects to other scripts:
//   - Initialized by `BeloteCard.Spawn()` and managed by
//     `GameStageRenderer` for layout.
//   - Uses `CardStaticData` to fetch the right sprite.
//   - Interacts with `Picker.Instance` for mouse world position.
//----------------------------------------------
public class CardComponent : MonoBehaviour
{
    //----------------------------------------------
    // Variables
    private BeloteCard m_card;                 // Backing model
    private bool m_isHovered = false;          // Mouse hover state
    private bool m_isSelected = false;         // Drag selection state
    private Vector3 m_initialPosition = new Vector3(); // Resting position

    //----------------------------------------------
    // Properties

    public bool Hovered
    {
        get { return m_isHovered; }
    }

    public bool Selected
    {
        get { return m_isSelected; }
    }

    public BeloteCard Card
    {
        get { return m_card; }
    }

    // Use this for initialization
    void Start()
    {
        
    }

    public void Init(BeloteCard card)
    {
        m_card = card;                         // Bind model
        m_isHovered = false;                   // Clear UI states
        m_isSelected = false;

        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        if(spriteRenderer != null)
        {
            spriteRenderer.sprite = CardStaticData.Instance.GetSprite(m_card.Value, m_card.Family); // Set correct sprite
        }

        gameObject.name = m_card.Value + " of " + m_card.Family; // Helpful for debugging in hierarchy
    }

    public void SetInitialPosition(Vector3 initialPosition)
    {
        transform.localPosition = initialPosition; // Place at anchor
        m_initialPosition = initialPosition;       // Remember for snapping back
    }

    bool CanBeSelected()
    {
        if(m_card != null)
        {
            bool isHuman = m_card.Owner as HumanPlayer != null; // Only human can select
            if(isHuman)
            {
                Player player = m_card.Owner as Player;
                return player.CanPlay(m_card);   // Check legality
            }
        }
        return false;   
    }
    

    // Update is called once per frame
    void Update()
    {
        if(CanBeSelected())
        {
            GameObject underMouse = Picker.Instance.UnderMouse; // Raycasted object under mouse

            if (underMouse != null && underMouse == gameObject)
            {
                SetHovered(true);            // Start hover
            }
            else
            {
                SetHovered(false);           // End hover
            }

            if(Hovered && !Selected)
            {
                if (Input.GetMouseButtonDown(0))
                {
                    SetSelected(true);       // Begin drag
                }
            }

            if (Selected && Input.GetMouseButtonUp(0))
            {
                SetSelected(false);          // End drag
            }

            if(m_isSelected)
            {
                Vector3 newPos = new Vector3();
                newPos = Picker.Instance.MouseWorldPos; // Follow mouse
                newPos.z -= 0.1f;
                transform.localPosition = newPos;
            }
            else
            {
                transform.localPosition = m_initialPosition; // Snap back to rest
            }
        }
        else
        {
            if(Selected)
            {
                SetSelected(false);          // Cancel selection if no longer valid
            }
        }
    }

    static float scaleFactor = 1.2f;
    static float invScaleFactor = 1.0f / scaleFactor;
    protected void SetHovered(bool under)
    {
        if (under != m_isHovered)
        {
            if (m_isHovered)
            {
                Vector3 scale = gameObject.transform.localScale;
                scale.x *= invScaleFactor;
                scale.y *= invScaleFactor;
                gameObject.transform.localScale = scale;
            }
            m_isHovered = under;
            if (m_isHovered)
            {
                Vector3 scale = gameObject.transform.localScale;
                scale.x *= scaleFactor;
                scale.y *= scaleFactor;
                gameObject.transform.localScale = scale;
            }
        }
    }

    protected void SetSelected(bool selected)
    {
        if(selected != m_isSelected)
        {
            bool isInHandArea = IsMouseInHandArea();

            m_isSelected = selected;

            BeloteCard.Selected evt = Pools.Claim<BeloteCard.Selected>();
            evt.Init(m_card, m_isSelected, isInHandArea);
            GameEventDispatcher.SendEvent(evt); // Notify UI/gameplay listeners
        }
    }

    protected bool IsMouseInHandArea()
    {
        Vector3 mouseToInitial = Input.mousePosition - gameObject.transform.position; // Screen-space distance
        return mouseToInitial.magnitude >= 1.0f; // Outside threshold => considered outside hand area
    }
}