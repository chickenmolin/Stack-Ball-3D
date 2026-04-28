using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Player : MonoBehaviour {
    private Rigidbody _rb;
    private float _overpowerBuildUp;       // Thanh tích năng lượng (0~1)

    [SerializeField] private bool _isClicked, _isOverPowered;
    [SerializeField] private float _moveSpeed = 500f;
    private float _speedLimit = 5f;        // Giới hạn tốc độ nảy lên
    [SerializeField] private float _bounceSpeed = 250f;

    public enum PlayerState { Prepare, Play, Dead, Finish }
    public PlayerState playerState = PlayerState.Prepare;

    private int currentBrokenPlatforms, totalPlatforms;

    // UI Overpower
    public GameObject _overpowerBar;
    public Image _overpowerFill;
    public GameObject _fireEffect;         // Hiệu ứng lửa khi overpower

    void Start() {
        totalPlatforms = FindObjectsOfType<PlatformController>().Length;
    }

    void Update() {
        if (playerState == PlayerState.Play) {
            ClickCheck();
            OverpowerCheck();
        }
        // Finish: click để sang màn tiếp
        if (playerState == PlayerState.Finish && Input.GetMouseButtonDown(0))
            FindObjectOfType<LevelSpawner>().IncreaseTheLevel();
    }

    void FixedUpdate() { BallMovement(); }

    private void BallMovement() {
        if (playerState == PlayerState.Play && Input.GetMouseButton(0) && _isClicked)
            _rb.velocity = new Vector3(0, -_moveSpeed * Time.fixedDeltaTime, 0); // Giữ chuột → đi xuống

        // Giới hạn tốc độ nảy lên
        if (_rb.velocity.y > _speedLimit)
            _rb.velocity = new Vector3(_rb.velocity.x, _speedLimit, _rb.velocity.z);
    }

    public void ClickCheck() {
        if (Input.GetMouseButtonDown(0))      _isClicked = true;
        else if (Input.GetMouseButtonUp(0))   _isClicked = false;
    }

    // Phá platform → cộng điểm (x2 nếu overpower)
    public void IncreaseScore() {
        currentBrokenPlatforms++;
        if (!_isOverPowered) { ScoreHandler.instance.AddScore(1); SoundManager.instance.PlaySoundEffect(breakClip, .5f); }
        else                 { ScoreHandler.instance.AddScore(2); SoundManager.instance.PlaySoundEffect(_overpowerBreakClip, .5f); }
    }

    void OnCollisionEnter(Collision target) {
        if (!_isClicked) {
            // Không giữ chuột → nảy lên + spawn hiệu ứng splash
            _rb.velocity = new Vector3(0, _bounceSpeed * Time.deltaTime, 0);
            if (!target.gameObject.CompareTag("Finish")) {
                GameObject splash = Instantiate(splashEffect);
                splash.transform.SetParent(target.transform);
                splash.transform.localEulerAngles = new Vector3(90, Random.Range(0, 359), 0);
                float s = Random.Range(0.18f, 0.25f);
                splash.transform.localScale = new Vector3(s, s, 1);
                splash.transform.position = new Vector3(transform.position.x, transform.position.y - 0.25f, transform.position.z);
                splash.GetComponent<SpriteRenderer>().color = GetComponent<MeshRenderer>().material.color;
            }
            SoundManager.instance.PlaySoundEffect(bounceClip, .5f);
        } else {
            if (_isOverPowered) {
                // Overpower → phá cả GoodPlatform lẫn BadPlatform
                if (target.gameObject.tag == "GoodPlatform" || target.gameObject.tag == "BadPlatform")
                    target.transform.parent.GetComponent<PlatformController>().BreakAllPlatforms();
            } else {
                if (target.gameObject.tag == "GoodPlatform")
                    target.transform.parent.GetComponent<PlatformController>().BreakAllPlatforms();
                if (target.gameObject.tag == "BadPlatform") {
                    // Chạm BadPlatform khi không overpower → chết
                    _rb.isKinematic = true;
                    transform.GetChild(0).gameObject.SetActive(false);
                    playerState = PlayerState.Dead;
                    SoundManager.instance.PlaySoundEffect(deadClip, .5f);
                }
            }
        }

        // Cập nhật thanh tiến trình
        FindObjectOfType<GameUI>().LevelSliderFill(currentBrokenPlatforms / (float)totalPlatforms);

        // Chạm đích → thắng
        if (target.gameObject.CompareTag("Finish") && playerState == PlayerState.Play) {
            SoundManager.instance.PlaySoundEffect(winClip, 1f);
            playerState = PlayerState.Finish;
            GameObject win = Instantiate(winEffect);
            win.transform.SetParent(Camera.main.transform);
            win.transform.localPosition = Vector3.up * 1.5f;
        }
    }

    // Tiếp tục nảy khi đứng yên trên platform
    void OnCollisionStay(Collision target) {
        if (!_isClicked || target.gameObject.CompareTag("Finish"))
            _rb.velocity = new Vector3(0, _bounceSpeed * Time.deltaTime, 0);
    }

    void OverpowerCheck() {
        if (_isOverPowered) {
            _overpowerBuildUp -= Time.deltaTime * .3f; // Hao dần khi overpower
            _fireEffect.SetActive(true);
        } else {
            _fireEffect.SetActive(false);
            _overpowerBuildUp += _isClicked                    // Giữ chuột → tăng
                ? Time.deltaTime * .8f
                : -Time.deltaTime * .5f;                       // Thả chuột → giảm
        }

        // Hiện thanh overpower khi tích đủ 30%
        _overpowerBar.SetActive(_overpowerBuildUp >= 0.3f || _overpowerFill.color == Color.red);

        // Kích hoạt / tắt overpower
        if (_overpowerBuildUp >= 1) { _overpowerBuildUp = 1; _isOverPowered = true;  _overpowerFill.color = Color.red; }
        else if (_overpowerBuildUp <= 0) { _overpowerBuildUp = 0; _isOverPowered = false; _overpowerFill.color = Color.white; }

        if (_overpowerBar.activeInHierarchy)
            _overpowerFill.fillAmount = _overpowerBuildUp;
    }
}
