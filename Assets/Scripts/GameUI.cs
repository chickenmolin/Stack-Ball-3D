using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class GameUI : MonoBehaviour {
    // Thanh tiến trình màn chơi
    public Image levelSlider, levelSliderFill;
    public Image currentLevelImg, nextLevelImg;

    // Các màn hình UI theo trạng thái game
    public GameObject firstUI, inGameUI, finishUI, gameOverUI;
    public GameObject allButtons; // Panel cài đặt ẩn/hiện
    private bool _buttons;        // Trạng thái panel cài đặt

    public Text currentLevelText, nextLevelText, finishLevelText,
                gameOverScoreText, gameOverBestText;

    private Player _player;
    private Material _playerMaterial;

    public Button soundButton;
    public Sprite soundOnImg, soundOffImg;

    void Awake() {
        _player = FindObjectOfType<Player>();
        _playerMaterial = _player.GetComponent<MeshRenderer>().material;

        // Tô màu UI theo màu nhân vật
        levelSlider.color     = _playerMaterial.color;
        levelSliderFill.color = _playerMaterial.color + Color.gray;
        nextLevelImg.color    = _playerMaterial.color;
        currentLevelImg.color = _playerMaterial.color;

        soundButton.onClick.AddListener(() => SoundManager.instance.SoundOnOff());
    }

    void Start() {
        int level = FindObjectOfType<LevelSpawner>()._level;
        currentLevelText.text = level.ToString();
        nextLevelText.text    = (level + 1).ToString();
    }

    void Update() { UIManagement(); }

    private void UIManagement() {
        // Cập nhật icon sound on/off
        if (_player.playerState == Player.PlayerState.Prepare) {
            soundButton.GetComponent<Image>().sprite = SoundManager.instance._soundPlay
                ? soundOnImg : soundOffImg;
        }

        // Click chuột → bắt đầu game (bỏ qua nếu click trúng UI)
        if (Input.GetMouseButtonDown(0) && !IgnoreUI() && _player.playerState == Player.PlayerState.Prepare) {
            _player.playerState = Player.PlayerState.Play;
            firstUI.SetActive(false); inGameUI.SetActive(true);
            finishUI.SetActive(false); gameOverUI.SetActive(false);
        }

        // Hoàn thành màn → hiện UI thắng
        if (_player.playerState == Player.PlayerState.Finish) {
            firstUI.SetActive(false); inGameUI.SetActive(false);
            finishUI.SetActive(true); gameOverUI.SetActive(false);
            finishLevelText.text = "Level " + FindObjectOfType<LevelSpawner>()._level;
        }

        // Chết → hiện UI thua + click để chơi lại
        if (_player.playerState == Player.PlayerState.Dead) {
            firstUI.SetActive(false); inGameUI.SetActive(false);
            finishUI.SetActive(false); gameOverUI.SetActive(true);
            gameOverScoreText.text = ScoreHandler.instance.score.ToString();
            gameOverBestText.text  = PlayerPrefs.GetInt("Highscore").ToString();
            if (Input.GetMouseButtonDown(0)) {
                ScoreHandler.instance.ResetScore();
                SceneManager.LoadScene(0); // Reload màn chơi
            }
        }
    }

    // Kiểm tra click có trúng UI element không (trừ IgnoreUI tag)
    private bool IgnoreUI() {
        PointerEventData eventData = new PointerEventData(EventSystem.current) { position = Input.mousePosition };
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, results);
        results.RemoveAll(r => r.gameObject.GetComponent<IgnoreUI>() != null);
        return results.Count > 0;
    }

    public void LevelSliderFill(float fillAmount) { levelSlider.fillAmount = fillAmount; }
    public void Settings() { allButtons.SetActive(_buttons = !_buttons); } // Toggle panel cài đặt
}
