using UnityEngine.SceneManagement;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine;

public class MenuManager : MonoBehaviour
{
  [Header("Painéis da UI")]
  [SerializeField] private RectTransform _mainMenuPanel;
  [SerializeField] private RectTransform _optionsPanel;

  [Header("Configurações de UI")]
  [SerializeField] private Slider _volumeSlider;
  [SerializeField] private string _sceneName;

  // Start is called once before the first execution of Update after the MonoBehaviour is created
  void Start()
  {
    _optionsPanel.gameObject.SetActive(false);
    _optionsPanel.localScale = Vector3.zero;
    _mainMenuPanel.gameObject.SetActive(true);
    _mainMenuPanel.localScale = Vector3.zero;

    _mainMenuPanel.DOScale(Vector3.one, 0.5f).SetEase(Ease.OutBack);

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
  public void OnVolumeChanged(float volume) => SetVolume(volume);

  private void OpenOptions()
  {
    _mainMenuPanel.DOScale(Vector3.zero, 0.3f).SetEase(Ease.InBack).OnComplete(() =>
    {
      _mainMenuPanel.gameObject.SetActive(false);
      _optionsPanel.gameObject.SetActive(true);
      _optionsPanel.DOScale(Vector3.one, 0.3f).SetEase(Ease.OutBack);
    });
  }
  public void Open() => OpenOptions();

  private void CloseOptions()
  {
    PlayerPrefs.Save();

    _optionsPanel.DOScale(Vector3.zero, 0.3f).SetEase(Ease.InBack).OnComplete(() =>
    {
      _optionsPanel.gameObject.SetActive(false);
      _mainMenuPanel.gameObject.SetActive(true);

      _mainMenuPanel.DOScale(Vector3.one, 0.3f).SetEase(Ease.OutBack);
    });
  }
  public void Close() => CloseOptions();

  private void PlayGame()
  {
    PlayerPrefs.Save();

    _mainMenuPanel.DOScale(Vector3.zero, 0.3f).SetEase(Ease.InBack).OnComplete(() =>
    {
      SceneManager.LoadScene(_sceneName);
    });
  }
  public void Play() => PlayGame();

  private void QuitGame()
  {
    _mainMenuPanel.DOScale(Vector3.zero, 0.3f).SetEase(Ease.InBack).OnComplete(() =>
    {
      Debug.Log("Fechando o jogo");
      Application.Quit();
    });
  }
  public void Quit() => QuitGame();
}
