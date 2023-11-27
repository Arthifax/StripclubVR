using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;
using UnityEngine.UI;

public class MoneyController : MonoBehaviour
{
    private int _currentAmountMoney = 0;
    private int _totalAmountMoneyGiven = 0;
    private int _amountClipsPlayed = 0;

    private bool _allClipsPlayed = false;

    private bool _toFirstVideo = false;
    public VideoClip firstVideo = null;

    private bool _paidDancer = false;

    bool doneFading = true;

    [SerializeField]
    private GameObject _nextLevelButton = null;
    [SerializeField]
    private GameObject _payDancerButton = null;

    private VideoManager vm = null;

    [SerializeField] private int _startingMoney = 1000;

    [SerializeField] private Text _moneyGivenText = null;
    [SerializeField] private Text _moneyOwnedText = null;
    [SerializeField] private string _moneyOwnedString = "Geld Over: ";
    [SerializeField] private string _moneyGivenString = "Geld Gegeven: ";

    [SerializeField] private Canvas _moneyCanvas = null;
    //[SerializeField] private GameObject panel = null;

    [SerializeField] private GameObject[] _moneyPiles = null;

    [SerializeField] private AudioClip _victorySFX = null;
    [SerializeField] private AudioClip _correctSFX = null;
    [SerializeField] private AudioClip _moneySFX = null;
    [SerializeField] private AudioClip _wrongSFX = null;
    [SerializeField] private AudioSource source = null;
    private bool _victoryPlayed = false;

    [SerializeField] private GameObject _info200 = null;
    [SerializeField] private GameObject _info250 = null;
    [SerializeField] private GameObject _info500 = null;
    [SerializeField] private GameObject _info1500 = null;
    [SerializeField] private GameObject _infoBg = null;


    [System.Serializable]
    protected class Act
    {
        public int price = 0;
        public VideoClip clip = null;
        public bool hasPlayed = false;
    }

    [SerializeField] private List<Act> acts = new List<Act>();

    private void Awake()
    {
        SetTotalAmountMoney(_startingMoney);
        GetComponent<Level1Manager>().SetLastVideo(firstVideo);
        vm = FindObjectOfType<VideoManager>();
        CheckMoneyPiles();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.O))
            _currentAmountMoney = 0;

        if (_allClipsPlayed)
        {
            if (vm.vp.time >= vm.vp.clip.length - .1f)
            {
                // Play victory sound
                if (_victoryPlayed) { }
                else
                {
                    // Go back to end video  1
                    vm.PlayVideo(firstVideo);
                    vm.vp.time = vm.vp.clip.length - .1f;
                    // Play victory sound
                    PlaySound(_victorySFX, .7f);
                    _victoryPlayed = true;
                }

                // Show information
                _info200.SetActive(true);
                _info250.SetActive(true);
                _info500.SetActive(true);
                _info1500.SetActive(true);
                _infoBg.SetActive(true);
                // Show button next level
                Debug.Log("Next Level Loaded!");
                if (_nextLevelButton && !_nextLevelButton.activeSelf)
                    _nextLevelButton.SetActive(true);
                else if (!_nextLevelButton)
                    GameManager.NextLevel();
            }
            _moneyCanvas.gameObject.SetActive(true);
        }

        if (!_payDancerButton.activeSelf && !_allClipsPlayed)
        {
            if (vm.vp.time >= vm.vp.clip.length - .1f)
            {
                _payDancerButton.SetActive(true);
                //_payDancerButton.GetComponentInChildren<Buttonlinker>().LowLight();
                _infoBg.SetActive(true);
                _moneyCanvas.gameObject.SetActive(true);
            }
        }

        if ((vm.vp.time >= vm.vp.clip.length - .1f) && _toFirstVideo)
        {
            if (_paidDancer)
            {
                PlaySound(_correctSFX, .7f);
                _paidDancer = false;
            }
            if (vm.vp.clip == acts[0].clip)
                _info200.SetActive(true);
            else if (vm.vp.clip == acts[1].clip)
                _info250.SetActive(true);
            else if (vm.vp.clip == acts[2].clip)
                _info500.SetActive(true);
            else if (vm.vp.clip == acts[3].clip)
                _info1500.SetActive(true);
            vm.PlayVideo(firstVideo);
            vm.vp.time = vm.vp.clip.length - .1f;
            _toFirstVideo = false;
        }
    }

    public void CheckMoneyPiles()
    {
        if (_moneyPiles.Length < 1)
            return;

        if (_currentAmountMoney < 50)
        {
            // Show nothing
            for (int i = 0; i < _moneyPiles.Length; i++)
            {
                _moneyPiles[i].SetActive(false);
            }
        }
        else if (_currentAmountMoney >= 50 && _currentAmountMoney < 100)
        {
            // Show 50
            for (int i = 0; i < _moneyPiles.Length; i++)
            {
                if (i == 0)
                    _moneyPiles[i].SetActive(true);
                else
                    _moneyPiles[i].SetActive(false);
            }
        }
        else if (_currentAmountMoney >= 100 && _currentAmountMoney < 200)
        {
            // Show 50, 100
            for (int i = 0; i < _moneyPiles.Length; i++)
            {
                if (i <= 1)
                    _moneyPiles[i].SetActive(true);
                else
                    _moneyPiles[i].SetActive(false);
            }
        }
        else if (_currentAmountMoney >= 200 && _currentAmountMoney < 500)
        {
            // Show 50, 100, 200
            for (int i = 0; i < _moneyPiles.Length; i++)
            {
                if (i <= 2)
                    _moneyPiles[i].SetActive(true);
                else
                    _moneyPiles[i].SetActive(false);
            }
        }
        else if (_currentAmountMoney >= 500)
        {
            // Show all
            for (int i = 0; i < _moneyPiles.Length; i++)
            {
                _moneyPiles[i].SetActive(true);
            }
        }
    }

    public void PlayNextAct()
    {
        if (_totalAmountMoneyGiven == 0)
        {
            PlaySound(_wrongSFX, .7f);
            return;
        }

        MoneyObject[] cashblocks = FindObjectsOfType<MoneyObject>();

        if (cashblocks.Length > 0)
        {
            for (int i = 0; i < cashblocks.Length; i++)
            {
                if (cashblocks[i].hasUsed)
                    continue;

                //_currentAmountMoney += cashblocks[i]._worth;
                Destroy(cashblocks[i].gameObject);
            }
        }

        for (int i = 0; i < acts.Count; i++)
        {
            if (_totalAmountMoneyGiven == acts[i].price && !acts[i].hasPlayed)
            {
                _paidDancer = true;
                vm.PlayVideo(acts[i].clip);
                GetComponent<Level1Manager>().SetLastVideo(acts[i].clip);
                acts[i].hasPlayed = true;
                _totalAmountMoneyGiven = 0;

                _amountClipsPlayed = 0;

                PlaySound(_moneySFX, .7f);


                for (int j = 0; j < acts.Count; j++)
                {
                    if (acts[j].hasPlayed)
                        _amountClipsPlayed++;

                    if (_amountClipsPlayed >= acts.Count - 1)
                        _allClipsPlayed = true;
                }
                if (!_allClipsPlayed)
                {
                    _toFirstVideo = true;
                }

                break;
            }
            else if (acts[i].price == 0)
            {
                PlaySound(_wrongSFX, .7f);
                // Not given enough moneys
                vm.PlayVideo(acts[i].clip);
                GetComponent<Level1Manager>().SetLastVideo(acts[i].clip);
                _toFirstVideo = true;
                _currentAmountMoney += _totalAmountMoneyGiven;
                _totalAmountMoneyGiven = 0;
                CheckMoneyPiles();
            }
        }

        //_payDancerButton.SetActive(false);
        //_infoBg.SetActive(false);
        //_moneyCanvas.gameObject.SetActive(false);


        _moneyOwnedText.text = _moneyOwnedString + _currentAmountMoney;
        _moneyGivenText.text = _moneyGivenString + _totalAmountMoneyGiven;
    }

    public void SetTotalAmountMoney(int amount) { _currentAmountMoney = amount; CheckMoneyPiles(); _moneyOwnedText.text = _moneyOwnedString + _currentAmountMoney; }

    public void GiveMoneyToLovelyLady(int amount)
    {
        _totalAmountMoneyGiven += amount;
        _moneyGivenText.text = _moneyGivenString + _totalAmountMoneyGiven;
    }

    public bool GrabMoneyFromPile(int amount)
    {
        if (amount <= _currentAmountMoney)
        {
            _currentAmountMoney -= amount;
            _moneyOwnedText.text = _moneyOwnedString + _currentAmountMoney;
            return true;
        }
        else
            return false;
    }

    public void PutMoneyBackOnPile(int amount)
    {
        _currentAmountMoney += amount;
        _moneyOwnedText.text = _moneyOwnedString + _currentAmountMoney;
        CheckMoneyPiles();
    }

    private void PlaySound(AudioClip clip, float volume)
    {
        source.volume = volume;
        source.PlayOneShot(clip);
    }

    public int GetTotalMoneyInPlay() { return _totalAmountMoneyGiven + _currentAmountMoney; }
}
