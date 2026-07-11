using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine;
using DG.Tweening;

public class PauseManager : MonoBehaviour
{
  [Header("Panéis da UI")]
  [SerializeField] private RectTransform _pausePanel;

  [Header("Slider de Volume")]
  [SerializeField] private Slider _volumeSlider;

  [Header("Nome exato do menu principal")]
  [SerializeField] private string _mainScene = "Menu";
  [SerializeField] private bool _isPaused = false;

  // Start is called once before the first execution of Update after the MonoBehaviour is created
  void Start()
  {
    if (_pausePanel != null)
    {
      _pausePanel.gameObject.SetActive(false);
      _pausePanel.localScale = Vector3.zero;
    }

    float savedVolume = PlayerPrefs.GetFloat("GameVolume", 1f);
    AudioListener.volume = savedVolume;

    if (_volumeSlider != null)
    {
      _volumeSlider.value = savedVolume;
      _volumeSlider.onValueChanged.AddListener(SetVolume);
    }
  }

  private void SetVolume(float volume)
  {
    AudioListener.volume = volume;
    PlayerPrefs.SetFloat("GameVolume", volume);
  }
  public void Volume(float volume) => SetVolume(volume);

  private void QuitToMainMenu()
  {
    Time.timeScale = 1f;
    PlayerPrefs.Save();

    _pausePanel.DOScale(Vector3.zero, 0.3f).SetEase(Ease.InBack).SetUpdate(true).OnComplete(() =>
        {
          SceneManager.LoadScene(_mainScene);
        });
  }
  public void Quit() => QuitToMainMenu();

  private void PauseGame()
  {
    if (_isPaused) return;

    _isPaused = true;
    Time.timeScale = 0f;

    _pausePanel.gameObject.SetActive(true);
    _pausePanel.DOScale(Vector3.one, 0.3f).SetEase(Ease.OutBack).SetUpdate(true);
  }
  public void Pause() => PauseGame();

  private void ResumeGame()
  {
    if (!_isPaused) return;

    _pausePanel.DOScale(Vector3.zero, 0.3f).SetEase(Ease.InBack).SetUpdate(true).OnComplete(() =>
    {
      _pausePanel.gameObject.SetActive(false);
      Time.timeScale = 1f;
      _isPaused = false;
    });
  }
  public void Resume() => ResumeGame();
}
