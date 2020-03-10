using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ImageDownloadScript : MonoBehaviour {

    public bool onlyOnlineImages = true;
    public List<Texture2D> puzzleImageList;

    public TextAsset textURL;
	[HideInInspector] public string puzzleImageURL;

	[HideInInspector] public int totalPuzzleAmount;
	bool downloadingPack;
	[HideInInspector] public bool areLinksLoaded;
	string[] imageLinksLoaded;
	WWW[] wwwImage;
	public WWW wwwImageLinks;
	public AsyncOperation async;
	ControlUI controlUI;
	[HideInInspector] public bool isWifiConnected;
	[HideInInspector] public bool connectedOnline;
	bool downloadTimedOut;


	void Start(){
        if (onlyOnlineImages)
        {
            puzzleImageList.Clear();
        }
		controlUI = GetComponent<ControlUI> ();
		totalPuzzleAmount = PlayerPrefs.GetInt ("amountPuzzlesTotal", 0);
		StartCoroutine(CheckInternetRoutine ());

		puzzleImageURL = textURL.text;
	}

	IEnumerator CheckInternetRoutine(){
		while (onlyOnlineImages) {
			CheckInternet ();
			yield return new WaitForSeconds (60);
		}
	}

	public bool CheckInternet(){
        if (!onlyOnlineImages)
        {
            isWifiConnected = true;
            connectedOnline = true;
            return true;
        }
		if (Application.internetReachability == NetworkReachability.NotReachable) {
			connectedOnline = false;
			isWifiConnected = false;
		} else {
			connectedOnline = true;
			if (Application.internetReachability == NetworkReachability.ReachableViaLocalAreaNetwork) {
				isWifiConnected = true;
			} else {
				isWifiConnected = false;
			}
		}
		return connectedOnline;
	}

	public void LoadList(){
        if (!onlyOnlineImages)
        {
            GetImagesOffline();
            return;
        }
		if (!downloadingPack) {
			downloadingPack = true;
			StartCoroutine (DownloadListRoutine ());
		}
	}

    void GetImagesOffline()
    {
        totalPuzzleAmount = puzzleImageList.Count;

        int amount = 4 + Mathf.Clamp(controlUI.packsCargados, 0, 1);
        while (controlUI.posicionPrimero + amount > totalPuzzleAmount)
        {
            amount--;
            if (amount <= 0)
            {
                Debug.Log("It's trying to load more puzzles than we have.");
            }
        }

        for (int i = controlUI.posicionPrimero; i < controlUI.posicionPrimero + amount; i++)
        {
            if (i > totalPuzzleAmount - 1)
            {
                break;
            }
            controlUI.TextureToButtonDirect(i, puzzleImageList[i]);
        }
        controlUI.ActivarAnimBoton();
    }

	public void LoadPack () {
		if (!areLinksLoaded && wwwImageLinks != null && wwwImageLinks.bytesDownloaded > 0 && wwwImageLinks.isDone) {
			areLinksLoaded = true;
			string rawLinksList = wwwImageLinks.text;
			imageLinksLoaded = rawLinksList.Split (';');
			if (imageLinksLoaded [imageLinksLoaded.Length - 1].Trim() == "") {
				List<string> linksListAux = new List<string>();
				for (int i = 0; i < imageLinksLoaded.Length - 1; i++) {
					linksListAux.Add (imageLinksLoaded [i]);
				}
				imageLinksLoaded = new string[(imageLinksLoaded.Length - 1)];
				for (int i = 0; i < imageLinksLoaded.Length; i++) {
					imageLinksLoaded [i] = linksListAux [i];
				}
			}
			wwwImage = new WWW[imageLinksLoaded.Length];
			totalPuzzleAmount = imageLinksLoaded.Length;
			PlayerPrefs.SetInt("amountPuzzlesTotal", totalPuzzleAmount);
			StartCoroutine (DownloadPack ());
		} else {
			StartCoroutine (DownloadPack ());
			if (downloadingPack) {
				downloadingPack = false;
			}
		}
	}

	Texture2D LoadImageIntoPlayerPrefs(int puzzleNum){
		Texture2D loadedTexture = ReadTextureFromPlayerPrefs (puzzleNum);
		return loadedTexture;
	}


	void WriteTextureToPlayerPrefs (int puzzleNum, Texture2D tex){
		byte[] texByte = tex.EncodeToJPG ();
		string base64Tex = System.Convert.ToBase64String (texByte);
		PlayerPrefs.SetString ("puzzleGuardado" + puzzleNum, base64Tex);
		PlayerPrefs.Save ();
		Debug.Log("Image "+ puzzleNum +" has been saved.");
	}

	Texture2D ReadTextureFromPlayerPrefs (int puzzleNum){
		string base64Tex = PlayerPrefs.GetString ("puzzleGuardado" + puzzleNum, null);
		if (!string.IsNullOrEmpty (base64Tex)) {
			byte[] texByte = System.Convert.FromBase64String (base64Tex);
			Texture2D tex = new Texture2D (2, 2);
			if (tex.LoadImage (texByte)) {
				return tex;
			}
		}
		return null;
	}

	IEnumerator DownloadListRoutine(){
		wwwImageLinks = new WWW(puzzleImageURL+"?t="+Random.Range(0,1000));
		StartCoroutine (TimeLimitToListDownload ());
		yield return wwwImageLinks;
		LoadPack ();
	}

	IEnumerator DownloadPack(){
		int amount = 4 + Mathf.Clamp(controlUI.packsCargados,0,1);

		//Si empezó offline, sin lista, aquí la descarga cuando hay internet
		if (!areLinksLoaded && CheckInternet()) {
			LoadList ();
			yield break;
		}

		while (controlUI.posicionPrimero + amount > totalPuzzleAmount) {
			amount--;
			if (amount <= 0) {
				Debug.Log ("It's trying to load more puzzles than we have.");
			}
		}

		for (int i = controlUI.posicionPrimero; i < controlUI.posicionPrimero + amount; i++) {
			if (i > totalPuzzleAmount - 1) {
				break;
			}
			if (CheckInternet () && PlayerPrefs.GetString ("puzzleGuardado" + i, "") == "") {
				imageLinksLoaded [i] = imageLinksLoaded [i].Trim ();
				wwwImage [i] = new WWW (imageLinksLoaded [i]);
				StartCoroutine (DownloadTimeLimit (i));
				yield return wwwImage [i];
				if (downloadTimedOut || !string.IsNullOrEmpty(wwwImage[i].error)) {
					if (!string.IsNullOrEmpty (wwwImage [i].error)) {
						Debug.Log ("Download error: " + wwwImage [i].error+". Image "+i+" has not been loaded.");
					} else {
						Debug.Log ("It took so long to load. Image "+i+" has not been loaded.");
					}
					downloadTimedOut = false;
					for (int eraseErrors = i-1; eraseErrors >= controlUI.posicionPrimero; eraseErrors--) {
						puzzleImageList.RemoveAt (eraseErrors);
						controlUI.grayscalePuzzleImages.RemoveAt (eraseErrors);
					}
					controlUI.ErrorWhileDownloading ();
					yield break;
				}

				Debug.Log ("Downloading image " + i + " from that link: " + imageLinksLoaded [i]);
				WriteTextureToPlayerPrefs (i, wwwImage [i].texture);
				puzzleImageList.Add (wwwImage [i].texture);
				controlUI.TexturaABoton (i, puzzleImageList [i]);
			} else {
				if (PlayerPrefs.GetString ("puzzleGuardado" + i, "") != "") {
					puzzleImageList.Add (LoadImageIntoPlayerPrefs (i));

					Debug.Log ("Image " + i + " loaded from prefs.");
					controlUI.TexturaABoton (i, puzzleImageList [i]);
				} else {
					Debug.Log ("There is no internet nor saved image. This should never happen.");
				}
			}
			yield return null;
		}

		controlUI.ActivarAnimBoton ();
	}

	IEnumerator DownloadTimeLimit (int i){
		float timer = 0;
		float timeOut = 10f; //Tiempo maximo para poder descargar una imagen
		downloadTimedOut = false;

		while (!wwwImage [i].isDone) {
			if (timer > timeOut) {
				downloadTimedOut = true;
				wwwImage [i].Dispose ();
				break;
			}
			timer += Time.deltaTime;
			yield return null;
		}
	}

	IEnumerator TimeLimitToListDownload (){
		float timer = 0;
		float timeOut = 10f; //Tiempo maximo para poder descargar una imagen
		downloadTimedOut = false;

		while (!wwwImageLinks.isDone) {
			if (timer > timeOut) {
				downloadTimedOut = true;
				wwwImageLinks.Dispose ();
				break;
			}
			timer += Time.deltaTime;
			yield return null;
		}

		if (downloadTimedOut) {
			controlUI.ErrorWhileDownloading ();
		}
		yield return null;
	}
}
