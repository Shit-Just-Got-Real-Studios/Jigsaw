using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
#if PLATFORM_ANDROID
using UnityEngine.Advertisements;
#endif

public class ControlUI : MonoBehaviour {

    ImageDownloadScript imageDownloadScript;
    LanguageManager langManager;

    public GameObject panelMain;
    public GameObject panelSelection;
    public GameObject panelPreGame;
    public GameObject panelInGame;
    public GameObject panelComplete;

    public GameObject[] continueButton;
    public GameObject[] buttonsPack;
    public Image[] buttonImages;
    Image[] buttonBorders;
    public List<Texture2D> grayscalePuzzleImages;
    Color[] difficultyColors;

    public Button playButton;
    public int packsCargados = 0;
    public int posicionPrimero = 0;
    bool cargandoNuevoPack;
    public RectTransform panelScroll;
    ScrollRect panelScrollComponent;
    public Image[] smallHexOfThumbnail;
    public Image[] difficultyTick;
    int ultimaDificultadSeleccionada = 0;
    public int preselectedPuzzle;

    bool mensajeCheckeoWifi;
    public Image downloadPopUp;
    public Image connectPopUp;
    public Image connectPopUpInGame;
    public Image tutorialAlert;

    bool bgHelpAlreadyRequested;
    bool sortPiecesAlreadyRequested;


    bool helpEnabled;
    public SpriteRenderer backgroundHelp;

    bool menuInGameEnabled;
    public RectTransform arrowMenuInGame;
    public Button botonMenuInvisible;

    public GameObject seguroMenu;
    int ultimoNumImagen = -1;

    Sprite spritePreseleccionado;
    public Image helpThumbnail;

    public Text completeInfo;
    float startingTime;
    int tiempoEnPuzzle;

    float contadorTiempoAnuncio = 0;
    float tiempoParaAnuncio = 480; //8 minutos
    bool puedeMostrarAnuncio;
    bool puedeMostrarAnuncioExtra;

    public GameObject[] prefabsPuzzle;
    GameObject newPuzzle;

    public GameObject tickSort;
    public GameObject tickGuia;
    public GameObject notaTachada;

    Sprite[] imagenesAColor;
    Sprite[] imagenesEnGris;


    [Header("SFX")]
    public AudioSource clickSFX;
    public AudioSource completeSFX;
    public AudioSource pauseSFX;
    public AudioSource barajarSFX;
    public AudioSource fixFondoSFX;
    public AudioSource fixGrupoSFX;
    public AudioSource errorSFX;
    public AudioSource desplegarSFX;

    void Start() {
        imageDownloadScript = GetComponent<ImageDownloadScript>();
        langManager = GetComponent<LanguageManager>();
        panelScrollComponent = panelScroll.GetComponent<ScrollRect>();
        buttonBorders = new Image[buttonImages.Length];
        for (int i = 0; i < buttonImages.Length; i++) {
            buttonBorders[i] = buttonImages[i].transform.parent.parent.GetComponent<Image>();
        }

        imagenesAColor = new Sprite[buttonImages.Length];
        imagenesEnGris = new Sprite[buttonImages.Length];

        difficultyColors = new Color[6];
        difficultyColors[0] = new Color(0.93f, 0.93f, 0.93f, 1); //Gris
        difficultyColors[1] = new Color(0.73f, 1, 0.79f, 1); //Verde 
        difficultyColors[2] = new Color(1, 1, 0.73f, 1); //Amarillo 
        difficultyColors[3] = new Color(1, 0.87f, 0.73f, 1); //Naranja 
        difficultyColors[4] = new Color(1, 0.73f, 0.73f, 1); //Rojo
        difficultyColors[5] = new Color(0.87f, 0.73f, 1, 1); //Morado 
    }

    void Update() {
        contadorTiempoAnuncio += Time.deltaTime;
        if (contadorTiempoAnuncio >= tiempoParaAnuncio * 2 && !puedeMostrarAnuncioExtra) {
            puedeMostrarAnuncioExtra = true;
        } else if (contadorTiempoAnuncio >= tiempoParaAnuncio && !puedeMostrarAnuncio) {
            puedeMostrarAnuncio = true;
        }
    }

    void AjustarNuevoPack() {
        Debug.Log("There is a total of " + imageDownloadScript.totalPuzzleAmount + " images");
        if (packsCargados == 1) {
            for (int i = 0; i < buttonsPack[packsCargados - 1].transform.childCount; i++) {
                if (i + 1 > imageDownloadScript.totalPuzzleAmount) {
                    buttonsPack[0].transform.GetChild(i).gameObject.SetActive(false);
                }
            }
        } else if (packsCargados == 2) {
            for (int i = 0; i < buttonsPack[packsCargados - 1].transform.childCount; i++) {
                if (i + 1 + 4/*4 del primer pack*/ > imageDownloadScript.totalPuzzleAmount) {
                    buttonsPack[packsCargados - 1].transform.GetChild(i).gameObject.SetActive(false);
                }
            }
        } else if (packsCargados >= 3) {
            for (int i = 0; i < buttonsPack[packsCargados - 1].transform.childCount; i++) {
                if (i + 1 + 4 + (5 * (packsCargados - 2))/*5 de los siguientes pack, -2 porque ya vamos por el pack 3*/ > imageDownloadScript.totalPuzzleAmount) {
                    buttonsPack[packsCargados - 1].transform.GetChild(i).gameObject.SetActive(false);
                }
            }
        }

        AjustarLimitesScrollVertical();
    }

    void AjustarLimitesScrollVertical() {
        if (packsCargados >= 2 && panelScrollComponent.enabled == false) { //Activamos el mover por scroll a partir del segundo pack
            panelScrollComponent.enabled = true;
        }
        panelScroll.offsetMin = new Vector2(0, panelScroll.offsetMax.y + (-140 * (packsCargados - 1) - 20 * (packsCargados - 2))); //Calibrado para dejar un margen abajo
    }

    public void LoadMorePuzzles() {
        clickSFX.Play();
        if (!cargandoNuevoPack) {
            //PARA EL PACK 0
            if (packsCargados == 0) {
                playButton.interactable = false;
                if (!imageDownloadScript.CheckInternet()) {
                    bool tienePackCompletoEnPrefs = true;
                    for (int i = 0; i < 4; i++) {
                        if (PlayerPrefs.GetString("puzzleGuardado" + i, "") == "") {
                            tienePackCompletoEnPrefs = false;
                        }
                    }
                    if (tienePackCompletoEnPrefs) {
                        cargandoNuevoPack = true;
                        imageDownloadScript.LoadPack();
                        playButton.transform.GetChild(0).GetComponent<Animator>().enabled = true;
                    } else {
                        Debug.Log("No tiene todo el pack guardado");
                        errorSFX.Play();
                        ErrorWhileDownloading();
                    }
                } else {
                    bool tienePackCompletoEnPrefs = true;
                    for (int i = 0; i < 4; i++) {
                        if (PlayerPrefs.GetString("puzzleGuardado" + i, "") == "") {
                            tienePackCompletoEnPrefs = false;
                        }
                    }

                    if (tienePackCompletoEnPrefs) {
                        cargandoNuevoPack = true;
                        imageDownloadScript.LoadPack();
                        playButton.transform.GetChild(0).GetComponent<Animator>().enabled = true;
                    } else if (!imageDownloadScript.isWifiConnected && !mensajeCheckeoWifi) {
                        errorSFX.Play();
                        downloadPopUp.gameObject.SetActive(true);
                    } else if (!imageDownloadScript.areLinksLoaded) {
                        cargandoNuevoPack = true;
                        imageDownloadScript.LoadList();
                        playButton.transform.GetChild(0).GetComponent<Animator>().enabled = true;
                    } else {
                        cargandoNuevoPack = true;
                        imageDownloadScript.LoadPack();
                        playButton.transform.GetChild(0).GetComponent<Animator>().enabled = true;
                    }
                }
            } else { //PARA EL RESTO DE PACKS
                if (!imageDownloadScript.CheckInternet()) {
                    bool tienePackCompletoEnPrefs = true;
                    for (int i = 0; i < 5; i++) {
                        if (PlayerPrefs.GetString("puzzleGuardado" + (i - 1 + packsCargados * 5), "") == "") {
                            tienePackCompletoEnPrefs = false;
                        }
                    }
                    if (tienePackCompletoEnPrefs) {
                        cargandoNuevoPack = true;
                        imageDownloadScript.LoadPack();
                        continueButton[packsCargados - 1].transform.GetChild(0).GetComponent<Animator>().enabled = true;
                    } else {
                        Debug.Log("No tiene todo el pack guardado");
                        errorSFX.Play();
                        ErrorWhileDownloading();
                    }
                } else {
                    bool tienePackCompletoEnPrefs = true;
                    for (int i = 0; i < 5; i++) {
                        if (PlayerPrefs.GetString("puzzleGuardado" + (i - 1 + packsCargados * 5), "") == "") {
                            tienePackCompletoEnPrefs = false;
                        }
                    }
                    if (tienePackCompletoEnPrefs) {
                        cargandoNuevoPack = true;
                        imageDownloadScript.LoadPack();
                        continueButton[packsCargados - 1].transform.GetChild(0).GetComponent<Animator>().enabled = true;
                    } else if (!imageDownloadScript.isWifiConnected && !mensajeCheckeoWifi) {
                        errorSFX.Play();
                        downloadPopUp.gameObject.SetActive(true);
                    } else {
                        cargandoNuevoPack = true;
                        imageDownloadScript.LoadPack();
                        continueButton[packsCargados - 1].transform.GetChild(0).GetComponent<Animator>().enabled = true;
                    }
                }
            }
        }
    }

    public void AcceptWifiDownload() {
        mensajeCheckeoWifi = true;
        if (packsCargados == 0) {
            if (!imageDownloadScript.areLinksLoaded) {
                cargandoNuevoPack = true;
                imageDownloadScript.LoadList();
                playButton.transform.GetChild(0).GetComponent<Animator>().enabled = true;
            } else {
                cargandoNuevoPack = true;
                imageDownloadScript.LoadPack();
                playButton.transform.GetChild(0).GetComponent<Animator>().enabled = true;
            }
        } else {
            cargandoNuevoPack = true;
            imageDownloadScript.LoadPack();
            continueButton[packsCargados - 1].transform.GetChild(0).GetComponent<Animator>().enabled = true;
        }
        clickSFX.Play();
        downloadPopUp.gameObject.SetActive(false);
    }

    public void DeclineWifiDownload() {
        cargandoNuevoPack = false;
        if (packsCargados > 0) {
            continueButton[packsCargados - 1].transform.GetChild(0).GetComponent<Animator>().enabled = false;
            continueButton[packsCargados - 1].transform.GetChild(0).GetChild(0).GetComponent<Text>().enabled = true;
            continueButton[packsCargados - 1].transform.GetChild(0).GetChild(1).GetComponent<Image>().enabled = false;
        } else if (packsCargados == 0) {
            ReturnPlayButton();
            playButton.interactable = true;
        }
        clickSFX.Play();
        downloadPopUp.gameObject.SetActive(false);
    }

    public void ActivarAnimBoton() {
        Invoke("MostrarNuevoPack", 0.2f);
    }

    void MostrarNuevoPack() {
        desplegarSFX.Play();
        cargandoNuevoPack = false; //Pack already loaded
        if (packsCargados > 0) { //Disable previous button
            continueButton[packsCargados - 1].SetActive(false);
        }
        buttonsPack[packsCargados].SetActive(true);
        posicionPrimero = posicionPrimero + 4 + Mathf.Clamp(packsCargados, 0, 1);
        packsCargados++;
        AjustarNuevoPack();
        if (packsCargados == 1) { //Re-write PLAY in the first button when second pack is loaded c
            ReturnPlayButton();
        }
    }

    public void TextureToButtonDirect(int buttonNum, Texture imageTex)
    {
        Texture textureClone = Instantiate(imageTex) as Texture;
        Sprite spriteImage = ConvertirASprite(buttonNum, textureClone);
        buttonImages[buttonNum].sprite = spriteImage;
        buttonBorders[buttonNum].color = difficultyColors[PlayerPrefs.GetInt("puzzleCompleto" + buttonNum, 0)];
    }
	public void TexturaABoton(int numeroBoton, Texture imagenTex){
		Texture texbuttonImages = Instantiate (imagenTex) as Texture;
		Sprite spriteImagen = ConvertirASprite(numeroBoton, texbuttonImages);
		buttonImages[numeroBoton].sprite = spriteImagen;
		buttonBorders [numeroBoton].color = difficultyColors[PlayerPrefs.GetInt("puzzleCompleto"+numeroBoton, 0)];
	}
		
	Sprite ConvertirASprite(int numeroBoton, Texture imagenTex){
		Texture2D imagenTex2D = imagenTex as Texture2D;
		if(PlayerPrefs.GetInt("puzzleCompleto"+numeroBoton, 0) < 5){
            Debug.Log("NUM " + numeroBoton + " " + imagenTex2D.name);
			imagenTex2D = ConvertToGrayscale(imagenTex2D);
		}
		grayscalePuzzleImages.Add (imagenTex2D);
		Rect rect = new Rect (0, 0, imagenTex2D.width, imagenTex2D.height);
		return Sprite.Create (imagenTex2D, rect, Vector2.zero, imagenTex2D.width);
	}

	Sprite ConversionRapidaASprite(Texture imagenTex){
		Texture2D imagenTex2D = imagenTex as Texture2D;
		Rect rect = new Rect (0, 0, imagenTex2D.width, imagenTex2D.height);
		return Sprite.Create (imagenTex2D, rect, Vector2.zero, imagenTex2D.width);
	}

	Texture2D ConvertToGrayscale(Texture2D imagenTex2D)
	{
        //return imagenTex2D;
        imagenTex2D = Instantiate(imagenTex2D) as Texture2D;
		float lightness = 0.2f;
		Color32[] pixels = imagenTex2D.GetPixels32();
        for (int x=0;x<imagenTex2D.width;x++)
		{
			for (int y=0;y<imagenTex2D.height;y++)
            {
                Color32 pixel = pixels[x+y*imagenTex2D.width];
				int p =  ( (256 * 256 + pixel.r) * 256 + pixel.b) * 256 + pixel.g;
				int b = p % 256;
				p = Mathf.FloorToInt(p / 256);
				int g = p % 256;
				p = Mathf.FloorToInt (p / 256);
				int r = p % 256;
				float l = (0.2126f*r/255f) + 0.7152f*(g/255f) + 0.0722f*(b/255f);
				Color c = new Color(l+lightness,l+lightness,l+lightness,1); //
				imagenTex2D.SetPixel(x,y,c);
            }
        }
        imagenTex2D.Apply(true);
        return imagenTex2D;
	}

	public void CloseConnectPopUp(){
		clickSFX.Play ();
		connectPopUp.gameObject.SetActive (false);
		connectPopUpInGame.gameObject.SetActive (false);
	}

	void EnableTutorialPopUp(){
		if (panelInGame.activeSelf) {
			tutorialAlert.gameObject.SetActive (true);
		}
	}

	public void CloseTutorialAlertAbriendoMenuInGame(){
		CloseTutorialAlert ();
		EnableMenuInGame ();
	}
	public void CloseTutorialAlert(){
		clickSFX.Play ();
		PlayerPrefs.SetInt ("yaUsoBotonSort", 1);
		tutorialAlert.gameObject.SetActive (false);
	}

	public void ErrorWhileDownloading() {
		cargandoNuevoPack = false;
		if (packsCargados > 0) {
			continueButton [packsCargados - 1].transform.GetChild (0).GetComponent<Animator> ().enabled = false;
			continueButton [packsCargados - 1].transform.GetChild (0).GetChild (0).GetComponent<Text> ().enabled = true;
			continueButton [packsCargados - 1].transform.GetChild (0).GetChild (1).GetComponent<Image> ().enabled = false;
		} else if (packsCargados == 0) {
			ReturnPlayButton ();
            playButton.interactable = true;
		}
		errorSFX.Play ();
		connectPopUp.gameObject.SetActive (true);
	}

	void ReturnPlayButton(){
        playButton.transform.GetChild (0).GetComponent<Animator> ().enabled = false;
        playButton.transform.GetChild (0).GetChild (0).GetComponent<Text> ().enabled = true;
        playButton.transform.GetChild (0).GetChild (1).GetComponent<Image> ().enabled = false;
	}

	public void VolverAMenu(){
		#if PLATFORM_ANDROID
		if(puedeMostrarAnuncioExtra && Advertisement.IsReady ("rewardedVideo")) {
			contadorTiempoAnuncio = 0;
			puedeMostrarAnuncioExtra = false;
			puedeMostrarAnuncio = false;
			ShowRewardedAdExtra ();
		} else if(puedeMostrarAnuncio && Advertisement.IsReady ()) {
			contadorTiempoAnuncio = 0;
			puedeMostrarAnuncioExtra = false;
			puedeMostrarAnuncio = false;
			ShowAd ();
		}
		#endif
		clickSFX.Play ();
		panelMain.SetActive (true);
		panelSelection.SetActive (false);
		panelPreGame.SetActive (false);
		panelInGame.SetActive (false);
		panelComplete.SetActive (false);
	}

	public void SelectImage(int numImagen){
		clickSFX.Play ();


		int piezasColoreadas = 0;
		int difficultyCompleted = CargarSaveDificultades (numImagen);

		switch (difficultyCompleted) {
		case 1:
			piezasColoreadas = 2;
			break;
		case 2:
			piezasColoreadas = 5;
			break;
		case 3:
			piezasColoreadas = 7;
			break;
		case 4:
			piezasColoreadas = 10;
			break;
		case 5:
			piezasColoreadas = 12;
			break;
		default:
			break;
		}



		if (ultimoNumImagen != numImagen) {
			preselectedPuzzle = numImagen;
			spritePreseleccionado = ConversionRapidaASprite (imageDownloadScript.puzzleImageList [preselectedPuzzle]);

			if (imagenesAColor [numImagen] == null || imagenesEnGris [numImagen] == null) {
				Texture2D texturaColor = Instantiate (imageDownloadScript.puzzleImageList [numImagen]) as Texture2D;
				Texture2D texturaGrises = Instantiate (imageDownloadScript.puzzleImageList [numImagen]) as Texture2D;

				Sprite imagenAColor = ConversionRapidaASprite (texturaColor);
				Sprite imagenEnGris = ConversionRapidaASprite (ConvertToGrayscale (texturaGrises));
				imagenesAColor [numImagen] = imagenAColor;
				imagenesEnGris [numImagen] = imagenEnGris;
			}
		}

		for (int i = 0; i < smallHexOfThumbnail.Length; i++) {
			if (i < piezasColoreadas) {
				smallHexOfThumbnail [i].sprite = imagenesAColor [numImagen];
			} else {
				smallHexOfThumbnail [i].sprite = imagenesEnGris [numImagen];
			}
		}

		panelMain.SetActive (false);
		panelSelection.SetActive (true);
		panelPreGame.SetActive (false);
		panelInGame.SetActive (false);
		panelComplete.SetActive (false);

		ultimoNumImagen = numImagen;
	}

	int CargarSaveDificultades (int numImagen)
    {
        int completedDifficulty = PlayerPrefs.GetInt ("puzzleCompleto" + numImagen, 0);
		for (int i = 0; i < difficultyTick.Length; i++) {
			if (i < completedDifficulty) {
				difficultyTick [i].enabled = true;
			} else {
				difficultyTick [i].enabled = false;
			}
		}
		return completedDifficulty;
	}

	public void IniciarPuzzle(int dificultadSeleccionada){
		clickSFX.Play ();
		ultimaDificultadSeleccionada = dificultadSeleccionada;
		Vector3 puzzlePosition = Vector3.zero;
        switch (dificultadSeleccionada)
        {
            case 0:
                puzzlePosition = new Vector3(0.2f, -0.5f, 0);
                break;
            case 1:
                puzzlePosition = new Vector3(0.2f, -0.5f, 0);
                break;
            case 2:
                puzzlePosition = Vector3.zero;
                break;
            case 3:
                puzzlePosition = Vector3.zero;
                break;
            case 4:
                puzzlePosition = Vector3.zero;
                break;
        }

		GameObject newPuzzle = Instantiate (prefabsPuzzle [dificultadSeleccionada], puzzlePosition, prefabsPuzzle [dificultadSeleccionada].transform.rotation) as GameObject;
        newPuzzle.name = prefabsPuzzle [dificultadSeleccionada].name; //Remove "(Clone)" in his name
		this.newPuzzle = newPuzzle;
		panelMain.SetActive (false);
		panelSelection.SetActive (false);
		panelPreGame.SetActive (true);
		panelInGame.SetActive (false);
		panelComplete.SetActive (false);
		LoadTexture ();
		EnableBackgroundHelp ();
	}

	void LoadTexture(){
		GameObject[] puzzlePieces = GameObject.FindGameObjectsWithTag ("PiezaPuzzle");
		for (int i = 0; i < puzzlePieces.Length; i++) {
            puzzlePieces[i].GetComponent<Renderer> ().material.mainTexture = imageDownloadScript.puzzleImageList [preselectedPuzzle];
		}
	}

	public void StartPuzzle(){
		MovePieces movePieces = this.gameObject.GetComponent<MovePieces> ();
		movePieces.attachPieces = GameObject.FindGameObjectWithTag("MatrizPuzzle").GetComponent <AttachPieces> ();
		DisableBGHelp ();
		ShufflePieces (newPuzzle.transform); //Barajar Piezas
		startingTime = Time.time;
		panelMain.SetActive (false);
		panelSelection.SetActive (false);
		panelPreGame.SetActive (false);
		panelInGame.SetActive (true);
		panelComplete.SetActive (false);
		Invoke ("EnableControls", 0.5f);
		if (PlayerPrefs.GetInt("yaUsoBotonSort", 0) == 0) {
			Invoke ("EnableTutorialPopUp", 30);
		}
	}

	void EnableControls(){
		MovePieces movePieces = this.gameObject.GetComponent<MovePieces> ();
		movePieces.isPuzzlePlaying = true;
	}

	void ShufflePieces (Transform newPuzzle){ //Shuffle pieces
		this.gameObject.GetComponent<MovePieces> ().zPos = 0.0f;
		List <int> depthList = new List<int>();
		int depth = 0;
		foreach (Transform child in newPuzzle) {
            depth = Random.Range (-newPuzzle.childCount, 0);
			while (depthList.Contains (depth)) {
                depth++;
				if (depth > 0) {
                    depth = -newPuzzle.childCount;
				}
			}
            depthList.Add (depth);
			StartCoroutine (LerpChild (child, depth * 0.001f, 2.25f));
		}
		barajarSFX.Play ();
	}

	IEnumerator LerpChild(Transform piece, float depth, float speed){
		Vector3 startingPos = piece.position;
		Vector3 finalPos = new Vector3 (Random.Range (-2.6f, 2.6f), Random.Range (5.2f, 5.5f), depth);
		float t = 0;
		while(t < 0.5f){
			t += speed * Time.deltaTime;
            piece.position = Vector3.Lerp (startingPos, finalPos, t*2);
			yield return null;
		}
		yield return null;
	}

	public void SortRemainingPieces(){
		this.gameObject.GetComponent<MovePieces> ().zPos = 0.0f;
		PlayerPrefs.SetInt ("yaUsoBotonSort", 1);
		List <int> depthList = new List<int>();
		int depth = 0;
		foreach (Transform child in newPuzzle.transform) {
			if (child.tag != "PiezaColocada") {
				depth = Random.Range (-newPuzzle.transform.childCount, 0);
				while (depthList.Contains (depth)) {
					depth++;
					if (depth > 0) {
						depth = -newPuzzle.transform.childCount;
					}
				}
                depthList.Add (depth);
				StartCoroutine (LerpChild (child, depth * 0.001f, 5));
			}
		}
		barajarSFX.Play ();
	}

	public void SortBorderPieces(){
		if (sortPiecesAlreadyRequested) {
			SortBorderPiecesAccepted ();
		} else if (imageDownloadScript.CheckInternet ()) {
			#if PLATFORM_ANDROID
			if (Advertisement.IsReady ("rewardedVideo")) {
				ShowRewardedAdSortPieces ();
			} else if (Advertisement.IsReady ()) {
				ShowAd ();
				SortBorderPiecesAccepted ();
			} 
			#else
				SortBorderPiecesAccepted ();
			#endif
		} else {
			errorSFX.Play ();
			connectPopUpInGame.gameObject.SetActive (true);
		}
	}

	void SortBorderPiecesAccepted(){
		PlayerPrefs.SetInt ("yaUsoBotonSort", 1);
		sortPiecesAlreadyRequested = true;
		if (!IsInvoking ("ReiniciarsortPiecesAlreadyRequested")) {
			Invoke ("ReiniciarsortPiecesAlreadyRequested", 300); //5 minutes
		}
		List <int> depthList = new List<int> ();
		int depth = 0;
		foreach (Transform child in newPuzzle.transform) {
			if (child.tag != "PiezaColocada") {
				depth = Random.Range (-newPuzzle.transform.childCount, 0);
				while (depthList.Contains (depth)) {
					depth++;
					if (depth > 0) {
						depth = -newPuzzle.transform.childCount;
					}
				}
                depthList.Add (depth);
				StartCoroutine (LerpChildBordes (child, depth * 0.001f, 5));
			}
		}
		barajarSFX.Play ();
		UpdatePauseButtons ();
	}

	IEnumerator LerpChildBordes (Transform pieza, float depth, float velocidadRecogida){
		Vector3 posInicial = pieza.position;
		Vector3 posFinal;

		string dimensionesPuzzle = newPuzzle.name.Substring (5, newPuzzle.name.Length - 5);
		string[] dosDimensiones = dimensionesPuzzle.Split('x');
		int anchoPuzzle = int.Parse(dosDimensiones [0])-1;
		int altoPuzzle = int.Parse(dosDimensiones [1])-1;

		if (pieza.name.Contains ("_0x") || pieza.name.Contains ("x0") || pieza.name.Contains ("x" + anchoPuzzle) || pieza.name.Contains ("_"+altoPuzzle + "x")) {
			posFinal = new Vector3 (Random.Range (-2.6f, -1), Random.Range (5.2f, 5.5f), depth);
		} else {
			posFinal = new Vector3 (Random.Range (1, 2.6f), Random.Range (5.2f, 5.5f), depth);
		}


		float t = 0;
		while(t < 0.5f){
			t += velocidadRecogida * Time.deltaTime;
			pieza.position = Vector3.Lerp (posInicial, posFinal, t*2);
			yield return null;
		}
		yield return null;
	}

	public void VolverDesdePuzzle(bool confirmacion){
		clickSFX.Play ();
		if (confirmacion) {
			MovePieces movePieces = this.gameObject.GetComponent<MovePieces> ();
			movePieces.isPuzzlePlaying = false;
			if (newPuzzle != null) {
				Destroy (newPuzzle);
			}
			SelectImage (preselectedPuzzle);
		}
		seguroMenu.SetActive (false);
	}

	void EnableBackgroundHelp(){
		helpThumbnail.sprite = spritePreseleccionado;
	}


	void DisableBGHelp(){
		helpEnabled = false;
		backgroundHelp.sprite = null;

		UpdatePauseButtons ();
	}

	public void EnableBGHelp(){
		if (helpEnabled) {
			helpEnabled = false;
			backgroundHelp.sprite = null;
			UpdatePauseButtons ();
			return;
		}

		if (bgHelpAlreadyRequested) {
			clickSFX.Play ();
			EnableBGHelpAceptado ();
		} else if (imageDownloadScript.CheckInternet ()) {
			clickSFX.Play ();
			#if PLATFORM_ANDROID
			if (Advertisement.IsReady ("rewardedVideo")) {
				ShowRewardedAdMostrarBG ();
			} else if (Advertisement.IsReady ()) {
				ShowAd ();
				EnableBGHelpAceptado ();
			} 
			#else
				EnableBGHelpAceptado();
			#endif
		} else {
			errorSFX.Play ();
			connectPopUpInGame.gameObject.SetActive (true);
		}
	}

	void EnableBGHelpAceptado(){
		bgHelpAlreadyRequested = true;
		if (!IsInvoking ("ReiniciarbgHelpAlreadyRequested")) {
			Invoke ("ReiniciarbgHelpAlreadyRequested", 480); //8 minutos
		}
		helpEnabled = !helpEnabled;
		if (helpEnabled) {
			backgroundHelp.sprite = spritePreseleccionado;
		} else {
			backgroundHelp.sprite = null;
		}
		UpdatePauseButtons ();
	}

	public void EnableMenuInGame(){
		desplegarSFX.Play ();
		menuInGameEnabled = !menuInGameEnabled;
		botonMenuInvisible.gameObject.SetActive (menuInGameEnabled);

		MovePieces movePieces = this.gameObject.GetComponent<MovePieces> ();
		movePieces.isPuzzlePlaying = !menuInGameEnabled;

		if (menuInGameEnabled) {
			UpdatePauseButtons ();
			arrowMenuInGame.rotation = Quaternion.Euler(new Vector3(0,0,180));
			if (tutorialAlert.gameObject.activeSelf) {
				CloseTutorialAlert ();
			}
			StartCoroutine (SlideMenuInGame (true));
		} else {
			arrowMenuInGame.rotation = Quaternion.Euler(new Vector3(0,0,0));
			StartCoroutine (SlideMenuInGame (false));
		}
	}

	IEnumerator SlideMenuInGame(bool mostrarMenu){

		RectTransform panelRect = panelInGame.GetComponent<RectTransform> ();

		float posShownBottom = 0;
		float posShownTop = -235;
		float posHiddenBottom = -369;
		float posHiddenTop = -603;

		float transition = (mostrarMenu) ? 0 : 1;
		float transitionSpeed = 5;



		while (transition >= 0 && transition <= 1) {
			if (mostrarMenu) {
                transition += Time.deltaTime * transitionSpeed;
			} else {
                transition -= Time.deltaTime * transitionSpeed;
			}
			arrowMenuInGame.anchoredPosition = new Vector2 (0, Mathf.Lerp (15, 10, transition));
			panelRect.offsetMin = new Vector2(0,Mathf.Lerp(posHiddenBottom, posShownBottom, transition));
			panelRect.offsetMax = new Vector2(0,Mathf.Lerp(posHiddenTop, posShownTop, transition));
			yield return null;
		}
		yield return null;
	}

	public void VerifyButton(string opcion){
		EnableMenuInGame ();
		switch (opcion) {
		case "Menu":
			seguroMenu.SetActive (true);
			break;
		}
	}

	public void ActivarPanelCompleto(){
		panelMain.SetActive (false);
		panelSelection.SetActive (false);
		panelPreGame.SetActive (false);
		panelInGame.SetActive (false);
		panelComplete.SetActive (true);
		tiempoEnPuzzle = Mathf.RoundToInt(Time.time - startingTime);

		string horas = "";
		int intHoras = 0;
		string minutos = "";
		if (tiempoEnPuzzle >= 60*60) {
			horas = Mathf.Floor (tiempoEnPuzzle / 3600).ToString () + ":";
			intHoras = tiempoEnPuzzle / 3600;
		}
		if (tiempoEnPuzzle >= 60) {
			if (tiempoEnPuzzle >= 60 * 60) {
				minutos = Mathf.Floor ((tiempoEnPuzzle - (intHoras * 3600)) / 60).ToString ("00") + ":";
			} else {
				minutos = Mathf.Floor (tiempoEnPuzzle / 60).ToString () + ":";
			}
		}
		string segundos = (tiempoEnPuzzle % 60).ToString("00");

		string letraTiempo = (tiempoEnPuzzle < 3600) ? "m" : "";
		letraTiempo = (tiempoEnPuzzle < 60) ? "s" : letraTiempo;
		completeInfo.text = langManager.difficultyString+ " " + (ultimaDificultadSeleccionada + 1).ToString () + "\n\n"+langManager.timerString+ " " + horas + minutos + segundos + letraTiempo;
	}

	public void VolverAMenuTrasCompletar(){
		clickSFX.Play ();
		MovePieces movePieces = this.gameObject.GetComponent<MovePieces> ();
		movePieces.isPuzzlePlaying = false;
		if (newPuzzle != null) {
			Destroy (newPuzzle);
		}
		SelectImage (preselectedPuzzle);

		//COLOREAR SI SE ACABA DE COMPLETAR EN DIFICIL
		buttonBorders [preselectedPuzzle].color = difficultyColors[PlayerPrefs.GetInt("puzzleCompleto"+ preselectedPuzzle, 0)];
		if (PlayerPrefs.GetInt ("puzzleCompleto" + preselectedPuzzle, 0) == 5) {
			TexturaABoton (preselectedPuzzle, imageDownloadScript.puzzleImageList [preselectedPuzzle]);
		}

		#if PLATFORM_ANDROID
		if(puedeMostrarAnuncioExtra && Advertisement.IsReady ("rewardedVideo")) {
			contadorTiempoAnuncio = 0;
			puedeMostrarAnuncioExtra = false;
			puedeMostrarAnuncio = false;
			ShowRewardedAdExtra ();
		} else if(puedeMostrarAnuncio && Advertisement.IsReady ()) {
			contadorTiempoAnuncio = 0;
			puedeMostrarAnuncioExtra = false;
			puedeMostrarAnuncio = false;
			ShowAd ();
		}
		#endif
		panelMain.SetActive (true);
		panelSelection.SetActive (false);
		panelPreGame.SetActive (false);
		panelInGame.SetActive (false);
		panelComplete.SetActive (false);
	}

	void ReiniciarbgHelpAlreadyRequested(){
		bgHelpAlreadyRequested = false;
		UpdatePauseButtons ();
	}
	void ReiniciarsortPiecesAlreadyRequested(){
		sortPiecesAlreadyRequested = false;
		UpdatePauseButtons ();
	}



	public void ShowAd(){
		#if PLATFORM_ANDROID
		if (Advertisement.IsReady()){
			Advertisement.Show();
		}
		#endif
	}


	public void ShowRewardedAdSortPieces()
	{
		#if PLATFORM_ANDROID
		if (Advertisement.IsReady("rewardedVideo"))
		{
			var options = new ShowOptions { resultCallback = HandleShowResultSepararPiezas };
			Advertisement.Show("rewardedVideo", options);
		}
		#endif
	}
	#if PLATFORM_ANDROID
	private void HandleShowResultSepararPiezas(ShowResult result){
		switch (result)
		{
		case ShowResult.Finished:
			Debug.Log ("The ad was successfully shown.");
			//
			// YOUR CODE TO REWARD THE GAMER
			SortBorderPiecesAccepted();
			break;
		case ShowResult.Skipped:
			Debug.Log("The ad was skipped before reaching the end.");
			break;
		case ShowResult.Failed:
			Debug.LogError("The ad failed to be shown.");
			break;
		}
	}

	public void ShowRewardedAdMostrarBG()
	{
		if (Advertisement.IsReady("rewardedVideo"))
		{
			var options = new ShowOptions { resultCallback = HandleShowResultMostrarBG };
			Advertisement.Show("rewardedVideo", options);
		}
	}

	private void HandleShowResultMostrarBG(ShowResult result){
		switch (result)
		{
		case ShowResult.Finished:
			Debug.Log ("The ad was successfully shown.");
			//
			// YOUR CODE TO REWARD THE GAMER
			EnableBGHelpAceptado ();
			break;
		case ShowResult.Skipped:
			Debug.Log("The ad was skipped before reaching the end.");
			break;
		case ShowResult.Failed:
			Debug.LogError("The ad failed to be shown.");
			break;
		}
	}

	public void ShowRewardedAdExtra()
	{
		if (Advertisement.IsReady("rewardedVideo"))
		{
			var options = new ShowOptions { resultCallback = HandleShowResultExtra };
			Advertisement.Show("rewardedVideo", options);
		}
	}

	private void HandleShowResultExtra (ShowResult result){
		switch (result)
		{
		case ShowResult.Finished:
			break;
		case ShowResult.Skipped:
			Debug.Log("The ad was skipped before reaching the end.");
			break;
		case ShowResult.Failed:
			Debug.LogError("The ad failed to be shown.");
			break;
		}
	}
	#endif

	void UpdatePauseButtons(){
		#if PLATFORM_ANDROID
		if (sortPiecesAlreadyRequested) {
			tickSort.SetActive (false);
		} else {
			tickSort.SetActive (true);
		}
		if (!bgHelpAlreadyRequested && !helpEnabled){
			tickGuia.SetActive (true);
		} else {
			tickGuia.SetActive (false);
		}
		#else
			tickSort.SetActive (false);
			tickGuia.SetActive (false);
		#endif
	}
}