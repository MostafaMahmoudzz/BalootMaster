using UnityEngine;
using UnityEngine.UI;
using Pebble;
using TMPro;

//-------------------------------------------------------
// RoundEndScoreUI
//-------------------------------------------------------
// Purpose:
//   Displays the detailed scoring breakdown at the end of each round.
//   Shows raw points, division, multiplier application, and final scores.
//-------------------------------------------------------
public class RoundEndScoreUI : MonoBehaviour
{
    [Header("UI Panel")]
    public GameObject scorePanel;
    public float displayDuration = 5f;  // How long to show the panel
    
    [Header("Team 1 UI")]
    public TextMeshProUGUI team1NameText;
    public TextMeshProUGUI team1RawPointsText;
    public TextMeshProUGUI team1DividedScoreText;
    public TextMeshProUGUI team1MultiplierText;
    public TextMeshProUGUI team1FinalScoreText;
    public TextMeshProUGUI team1CumulativeText;
    public Image team1BidderIndicator;
    public Image team1WinnerIndicator;
    
    [Header("Team 2 UI")]
    public TextMeshProUGUI team2NameText;
    public TextMeshProUGUI team2RawPointsText;
    public TextMeshProUGUI team2DividedScoreText;
    public TextMeshProUGUI team2MultiplierText;
    public TextMeshProUGUI team2FinalScoreText;
    public TextMeshProUGUI team2CumulativeText;
    public Image team2BidderIndicator;
    public Image team2WinnerIndicator;
    
    [Header("General Info")]
    public TextMeshProUGUI multiplierText;
    public TextMeshProUGUI kabootText;
    public TextMeshProUGUI roundResultText;
    
    [Header("Colors")]
    public Color bidderColor = new Color(1f, 0.8f, 0.3f);  // Gold
    public Color winnerColor = new Color(0.3f, 1f, 0.3f);   // Green
    public Color loserColor = new Color(1f, 0.3f, 0.3f);    // Red
    
    private float hideTimer = -1f;
    
    //----------------------------------------------
    void Awake()
    {
        if (scorePanel != null)
        {
            scorePanel.SetActive(false);
        }
        
        // Subscribe to round end score event
        GameEventDispatcher.Subscribe<GameStage.RoundEndScoreEvent>(OnRoundEndScore);
    }
    
    //----------------------------------------------
    void OnDestroy()
    {
        GameEventDispatcher.UnSubscribe<GameStage.RoundEndScoreEvent>(OnRoundEndScore);
    }
    
    //----------------------------------------------
    void Update()
    {
        if (hideTimer > 0)
        {
            hideTimer -= Time.deltaTime;
            if (hideTimer <= 0)
            {
                HideScorePanel();
            }
        }
    }
    
    //----------------------------------------------
    void OnRoundEndScore(GameStage.RoundEndScoreEvent evt)
    {
        Debug.Log("[RoundEndScoreUI] Received RoundEndScoreEvent - displaying scores");
        DisplayScores(evt);
    }
    
    //----------------------------------------------
    public void DisplayScores(GameStage.RoundEndScoreEvent evt)
    {
        if (scorePanel == null)
        {
            Debug.LogWarning("[RoundEndScoreUI] Score panel is null!");
            return;
        }
        
        // Show panel
        scorePanel.SetActive(true);
        hideTimer = displayDuration;
        
        // Team names
        if (team1NameText != null)
            team1NameText.text = "Team 1 (South & North)";
        if (team2NameText != null)
            team2NameText.text = "Team 2 (West & East)";
        
        // Calculate divided scores (before multiplier)
        int team1Divided = evt.IsKaboot && evt.Team1RawPoints > 0 ? 16 : Mathf.RoundToInt(evt.Team1RawPoints / 10f);
        int team2Divided = evt.IsKaboot && evt.Team2RawPoints > 0 ? 16 : Mathf.RoundToInt(evt.Team2RawPoints / 10f);
        
        // Team 1 scores
        if (team1RawPointsText != null)
            team1RawPointsText.text = $"Raw Points: {evt.Team1RawPoints}";
        if (team1DividedScoreText != null)
            team1DividedScoreText.text = evt.IsKaboot && evt.Team1RawPoints > 0 ? 
                $"Kaboot: {team1Divided}" : $"÷ 10: {team1Divided}";
        if (team1MultiplierText != null)
            team1MultiplierText.text = evt.Multiplier > 1 ? $"× {evt.Multiplier}" : "";
        if (team1FinalScoreText != null)
        {
            team1FinalScoreText.text = $"Round: +{evt.Team1RoundScore}";
            team1FinalScoreText.color = evt.Team1RoundScore > 0 ? winnerColor : loserColor;
        }
        if (team1CumulativeText != null)
            team1CumulativeText.text = $"Total: {evt.Team1CumulativeScore}";
        
        // Team 2 scores
        if (team2RawPointsText != null)
            team2RawPointsText.text = $"Raw Points: {evt.Team2RawPoints}";
        if (team2DividedScoreText != null)
            team2DividedScoreText.text = evt.IsKaboot && evt.Team2RawPoints > 0 ? 
                $"Kaboot: {team2Divided}" : $"÷ 10: {team2Divided}";
        if (team2MultiplierText != null)
            team2MultiplierText.text = evt.Multiplier > 1 ? $"× {evt.Multiplier}" : "";
        if (team2FinalScoreText != null)
        {
            team2FinalScoreText.text = $"Round: +{evt.Team2RoundScore}";
            team2FinalScoreText.color = evt.Team2RoundScore > 0 ? winnerColor : loserColor;
        }
        if (team2CumulativeText != null)
            team2CumulativeText.text = $"Total: {evt.Team2CumulativeScore}";
        
        // Bidder indicators
        if (team1BidderIndicator != null)
        {
            team1BidderIndicator.gameObject.SetActive(evt.BiddingTeam == PlayerTeam.Team1);
            team1BidderIndicator.color = bidderColor;
        }
        if (team2BidderIndicator != null)
        {
            team2BidderIndicator.gameObject.SetActive(evt.BiddingTeam == PlayerTeam.Team2);
            team2BidderIndicator.color = bidderColor;
        }
        
        // Winner indicators
        if (team1WinnerIndicator != null)
        {
            team1WinnerIndicator.gameObject.SetActive(evt.WinningTeam == PlayerTeam.Team1);
            team1WinnerIndicator.color = winnerColor;
        }
        if (team2WinnerIndicator != null)
        {
            team2WinnerIndicator.gameObject.SetActive(evt.WinningTeam == PlayerTeam.Team2);
            team2WinnerIndicator.color = winnerColor;
        }
        
        // General info
        if (multiplierText != null)
        {
            if (evt.Multiplier > 1)
            {
                multiplierText.gameObject.SetActive(true);
                multiplierText.text = $"Multiplier: ×{evt.Multiplier}";
                multiplierText.color = Color.yellow;
            }
            else
            {
                multiplierText.gameObject.SetActive(false);
            }
        }
        
        if (kabootText != null)
        {
            if (evt.IsKaboot)
            {
                kabootText.gameObject.SetActive(true);
                kabootText.text = "KABOOT! (Won all tricks)";
                kabootText.color = Color.yellow;
            }
            else
            {
                kabootText.gameObject.SetActive(false);
            }
        }
        
        if (roundResultText != null)
        {
            bool bidderWon = (evt.BiddingTeam == evt.WinningTeam);
            string bidderName = evt.BiddingTeam == PlayerTeam.Team1 ? "Team 1" : "Team 2";
            
            if (bidderWon)
            {
                roundResultText.text = $"{bidderName} (Bidder) won the round!";
                roundResultText.color = winnerColor;
            }
            else
            {
                roundResultText.text = $"{bidderName} (Bidder) lost - Opponent scores!";
                roundResultText.color = loserColor;
            }
        }
        
        Debug.Log("[RoundEndScoreUI] Score panel displayed successfully");
    }
    
    //----------------------------------------------
    public void HideScorePanel()
    {
        if (scorePanel != null)
        {
            scorePanel.SetActive(false);
            Debug.Log("[RoundEndScoreUI] Score panel hidden");
        }
    }
    
    //----------------------------------------------
    // Manual close button
    public void OnCloseButtonClicked()
    {
        HideScorePanel();
    }
}


