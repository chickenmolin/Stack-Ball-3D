using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LevelSpawner : MonoBehaviour {
    [SerializeField] private GameObject[] _allPlatforms;          // Toàn bộ platform có thể dùng
    [SerializeField] private GameObject[] _selectedPlatforms = new GameObject[4]; // 4 platform được chọn cho màn này
    [SerializeField] private GameObject _winPrefab;               // Platform đích (cuối màn)

    public int _level = 1, _platformAddition = 7; // _platformAddition: số platform thêm cho màn đầu
    [SerializeField] private float _rotationSpeed = 10f;

    public Material plateMaterial, baseMaterial;
    public Image currentLevelImage, nextLevelImage, progressBarImage;
    public MeshRenderer playerMesh;

    void Awake() { LevelManagement(); }

    private void LevelManagement() {
        // Tạo màu ngẫu nhiên và áp dụng đồng bộ lên UI + vật liệu
        plateMaterial.color   = Random.ColorHSV(0, 1, .5f, 1, 1, 1);
        baseMaterial.color    = plateMaterial.color + Color.gray;
        playerMesh.material.color     = plateMaterial.color;
        currentLevelImage.color       = plateMaterial.color;
        nextLevelImage.color          = plateMaterial.color;
        progressBarImage.color        = plateMaterial.color;

        _level = PlayerPrefs.GetInt("Level", 1);
        if (_level > 9) _platformAddition = 0; // Màn cao không cộng thêm platform

        PlatformSelection(); // Chọn bộ platform theo màn

        // Spawn platform từ trên xuống, khoảng cách 0.5f, xoay dần theo _rotationSpeed
        for (float i = 0; i > -_level - _platformAddition; i -= 0.5f) {

            // Chọn độ khó platform theo level
            if (_level <= 40)        _normalPlatforms = Instantiate(_selectedPlatforms[Random.Range(0, 2)]);
            else if (_level <= 80)   _normalPlatforms = Instantiate(_selectedPlatforms[Random.Range(1, 3)]);
            else if (_level <= 140)  _normalPlatforms = Instantiate(_selectedPlatforms[Random.Range(2, 4)]);
            else                     _normalPlatforms = Instantiate(_selectedPlatforms[Random.Range(3, 4)]);

            _normalPlatforms.transform.position    = new Vector3(0, i, 0);
            _normalPlatforms.transform.eulerAngles = new Vector3(0, i * _rotationSpeed, 0);

            // Vùng giữa màn (30%~60%) xoay thêm 180° tạo biến thể bất ngờ
            if (Mathf.Abs(i) >= _level * .3f && Mathf.Abs(i) <= _level * .6f)
                _normalPlatforms.transform.eulerAngles += Vector3.up * 180;

            _normalPlatforms.transform.parent = FindObjectOfType<Platforms>().transform;
        }

        // Đặt platform đích ở cuối cùng
        _winPlatform = Instantiate(_winPrefab);
        _winPlatform.transform.position = new Vector3(0, i, 0);
    }

    // Chọn ngẫu nhiên 1 trong 5 bộ platform (mỗi bộ 4 loại khó dần)
    void PlatformSelection() {
        int randomModel = Random.Range(0, 5);
        for (int i = 0; i < 4; i++)
            _selectedPlatforms[i] = _allPlatforms[i + randomModel * 4];
    }

    // Lưu level tiếp theo và reload scene
    public void IncreaseTheLevel() {
        PlayerPrefs.SetInt("Level", PlayerPrefs.GetInt("Level") + 1);
        SceneManager.LoadScene(0);
    }
}
