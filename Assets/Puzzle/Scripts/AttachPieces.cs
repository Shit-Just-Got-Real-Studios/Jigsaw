using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttachPieces : MonoBehaviour {

	public Transform[,] piecesArray;
	public List <GameObject> unplacedPieces;

	float snapMargin;
	int puzzleWidth;
	int puzzleHeight;
	public Vector3[,] initialPiecePosArray;
	GameObject controllerObject;
	MovePieces movePieces;
	ControlUI controlUI;

	DistanceBetweenPieces[,] distanceBetweenPieces;
	public struct DistanceBetweenPieces {
		public float above;
		public float below;
		public float right;
		public float left;
	}


	void Start () {
		controllerObject = GameObject.Find ("Controller");
		movePieces = controllerObject.GetComponent<MovePieces> ();
		controlUI = controllerObject.GetComponent<ControlUI> ();

		unplacedPieces = new List<GameObject> ();
		string puzzleDimensions = this.gameObject.name.Substring (5, this.gameObject.name.Length - 5);
		string[] twoDimensions = puzzleDimensions.Split('x');
		puzzleWidth = int.Parse(twoDimensions [0]);
		puzzleHeight = int.Parse(twoDimensions [1]);

		piecesArray = new Transform[puzzleWidth, puzzleHeight];
		initialPiecePosArray = new Vector3[puzzleWidth, puzzleHeight];
		distanceBetweenPieces = new DistanceBetweenPieces[puzzleWidth, puzzleHeight];

		snapMargin = 0.2f - (puzzleWidth * 0.01f); //Va desde 0.16 (El puzzle de 4) hasta 0.10 (El puzzle de 10)

		for (int i = 0; i < puzzleHeight; i++) {
			for (int k = 0; k < puzzleWidth; k++) {
				piecesArray [k, i] = transform.GetChild (i * puzzleWidth + k);
				initialPiecePosArray [k,i] = piecesArray [k, i].position;
				unplacedPieces.Add (piecesArray [k, i].gameObject);
			}
		}
		for (int i = 0; i < puzzleHeight; i++) {
			for (int k = 0; k < puzzleWidth; k++) {
				if (k > 0) {
					distanceBetweenPieces [k, i].left = Mathf.Abs(piecesArray [k, i].position.x - piecesArray [k - 1, i].position.x);
				}
				if (k < puzzleWidth - 1) {
					distanceBetweenPieces [k, i].right = Mathf.Abs(piecesArray [k + 1, i].position.x - piecesArray [k, i].position.x);
				}
				if (i > 0) {
					distanceBetweenPieces [k, i].above = Mathf.Abs(piecesArray [k, i - 1].position.y - piecesArray [k, i].position.y);
				}
				if (i < puzzleHeight - 1) {
					distanceBetweenPieces [k, i].below = Mathf.Abs(piecesArray [k, i].position.y - piecesArray [k, i + 1].position.y);
				}
			}
		}
	}

	public void ReadjustPieces(int k, int i){
		bool hasLeft = false;
		bool hasRight = false;
		bool hasAbove = false;
		bool hasBelow = false;

		if (k > 0 && distanceBetweenPieces [k, i].left <= Mathf.Abs (piecesArray [k, i].position.x - piecesArray [k - 1, i].position.x) + snapMargin
			&& distanceBetweenPieces [k, i].left >= Mathf.Abs (piecesArray [k, i].position.x - piecesArray [k - 1, i].position.x) - snapMargin
		     && piecesArray [k, i].position.y <= piecesArray [k - 1, i].position.y + snapMargin
		     && piecesArray [k, i].position.y >= piecesArray [k - 1, i].position.y - snapMargin
			 && piecesArray [k, i].position.x > piecesArray [k - 1, i].position.x) {
			hasLeft = true;

			Vector3 previousPosition = piecesArray [k, i].transform.position;
			piecesArray [k, i].transform.position = new Vector3 (piecesArray [k - 1, i].position.x + distanceBetweenPieces [k - 1, i].right, piecesArray [k - 1, i].position.y, piecesArray [k - 1, i].position.z);
			Vector3 currentPosition = piecesArray [k, i].transform.position;

			if (piecesArray [k, i].parent.name == "GrupoPiezas") {
				piecesArray [k, i].transform.position = previousPosition;
				piecesArray [k, i].parent.transform.position += currentPosition - previousPosition;
			}

			GroupUpPieces (k, i, "Left");
		}
		if (k < puzzleWidth - 1 && distanceBetweenPieces [k, i].right <= Mathf.Abs (piecesArray [k + 1, i].position.x - piecesArray [k, i].position.x) + snapMargin
			&& distanceBetweenPieces [k, i].right >= Mathf.Abs (piecesArray [k + 1, i].position.x - piecesArray [k, i].position.x) - snapMargin
		     && piecesArray [k, i].position.y <= piecesArray [k + 1, i].position.y + snapMargin
		     && piecesArray [k, i].position.y >= piecesArray [k + 1, i].position.y - snapMargin
			 && piecesArray [k, i].position.x < piecesArray [k + 1, i].position.x) {
			hasRight = true;

			Vector3 previousPosition = piecesArray [k, i].transform.position;
			piecesArray [k, i].transform.position = new Vector3 (piecesArray [k + 1, i].position.x - distanceBetweenPieces [k + 1, i].left, piecesArray [k + 1, i].position.y, piecesArray [k + 1, i].position.z);
			Vector3 currentPosition = piecesArray [k, i].transform.position;

			if (piecesArray [k, i].parent.name == "GrupoPiezas") {
				piecesArray [k, i].transform.position = previousPosition;
				piecesArray [k, i].parent.transform.position += currentPosition - previousPosition;
			}
			GroupUpPieces (k, i, "Right");
		}
		if (i > 0 && distanceBetweenPieces [k, i].above <= Mathf.Abs (piecesArray [k, i - 1].position.y - piecesArray [k, i].position.y) + snapMargin
			&& distanceBetweenPieces [k, i].above >= Mathf.Abs (piecesArray [k, i - 1].position.y - piecesArray [k, i].position.y) - snapMargin
		     && piecesArray [k, i].position.x <= piecesArray [k, i - 1].position.x + snapMargin
			 && piecesArray [k, i].position.x >= piecesArray [k, i - 1].position.x - snapMargin
			 && piecesArray [k, i].position.y < piecesArray [k, i - 1].position.y) {
			hasAbove = true;

			Vector3 previousPosition = piecesArray [k, i].transform.position;
			piecesArray [k, i].transform.position = new Vector3 (piecesArray [k, i - 1].position.x, piecesArray [k, i - 1].position.y - distanceBetweenPieces [k, i - 1].below, piecesArray [k, i - 1].position.z);
			Vector3 currentPosition = piecesArray [k, i].transform.position;

			if (piecesArray [k, i].parent.name == "GrupoPiezas") {
				piecesArray [k, i].transform.position = previousPosition;
				piecesArray [k, i].parent.transform.position += currentPosition - previousPosition;
			}
			GroupUpPieces (k, i, "Above");
		}
		if (i < puzzleHeight - 1 && distanceBetweenPieces [k, i].below <= Mathf.Abs (piecesArray [k, i].position.y - piecesArray [k, i + 1].position.y) + snapMargin
			&& distanceBetweenPieces [k, i].below >= Mathf.Abs (piecesArray [k, i].position.y - piecesArray [k, i + 1].position.y) - snapMargin
		     && piecesArray [k, i].position.x <= piecesArray [k, i + 1].position.x + snapMargin
			 && piecesArray [k, i].position.x >= piecesArray [k, i + 1].position.x - snapMargin
			 && piecesArray [k, i].position.y > piecesArray [k, i + 1].position.y) {
			hasBelow = true;

			Vector3 previousPosition = piecesArray [k, i].transform.position;
			piecesArray [k, i].transform.position = new Vector3 (piecesArray [k, i + 1].position.x, piecesArray [k, i + 1].position.y + distanceBetweenPieces [k, i + 1].above, piecesArray [k, i + 1].position.z);
			Vector3 currentPosition = piecesArray [k, i].transform.position;

			if (piecesArray [k, i].parent.name == "GrupoPiezas") {
				piecesArray [k, i].transform.position = previousPosition;
				piecesArray [k, i].parent.transform.position += currentPosition - previousPosition;
			}
			GroupUpPieces (k, i, "Below");
		}
		SnapPieceToGrid ();
		CheckIfCompleted ();
	}

	void SnapPieceToGrid(){
		bool anyPieceHasBeenPlaced = false;
		for (int i = 0; i < puzzleHeight; i++) {
			for (int k = 0; k < puzzleWidth; k++) { //Si no es PiezaColocada, o si lo es, pero está mal posicionada
				if (piecesArray [k, i].tag != "PiezaColocada" ||
				   (piecesArray [k, i].tag != "PiezaColocada"
				   && piecesArray [k, i].position.x != initialPiecePosArray [k, i].x
				   && piecesArray [k, i].position.y != initialPiecePosArray [k, i].y)) {
					if (Vector2.Distance (piecesArray [k, i].position, initialPiecePosArray [k, i]) < snapMargin) { 
						piecesArray [k, i].tag = "PiezaColocada";
						unplacedPieces.Remove (piecesArray [k, i].gameObject);
						piecesArray [k, i].GetChild (0).GetComponent<MeshRenderer> ().enabled = false;
						anyPieceHasBeenPlaced = true;
						if (piecesArray [k, i].parent.name == "GrupoPiezas") {
							Transform parentToDestroy = piecesArray [k, i].parent;
							while (parentToDestroy.childCount > 0) {
								parentToDestroy.GetChild (0).SetParent (parentToDestroy.parent);
							}
							if (parentToDestroy.childCount == 0) {
								Destroy (parentToDestroy.gameObject);
							}
						}
						piecesArray [k, i].position = new Vector3 (initialPiecePosArray [k, i].x, initialPiecePosArray [k, i].y, 1);
					}
				} 
			}
		}

		if (anyPieceHasBeenPlaced) {
			controlUI.fixFondoSFX.Play ();
		}
	}

	void CheckIfCompleted(){
		if (unplacedPieces.Count == 0) {
			switch (puzzleHeight) {
			case 4: //Noob
				if (PlayerPrefs.GetInt ("puzzleCompleto" + controlUI.preselectedPuzzle, 0) < 1) {
					PlayerPrefs.SetInt ("puzzleCompleto" + controlUI.preselectedPuzzle, 1);
				}
				break;
			case 6: //Facil
				if (PlayerPrefs.GetInt ("puzzleCompleto" + controlUI.preselectedPuzzle, 0) < 2) {
					PlayerPrefs.SetInt ("puzzleCompleto" + controlUI.preselectedPuzzle, 2);
				}
				break;
			case 10: //Medio
				if (PlayerPrefs.GetInt ("puzzleCompleto" + controlUI.preselectedPuzzle, 0) < 3) {
					PlayerPrefs.SetInt ("puzzleCompleto" + controlUI.preselectedPuzzle, 3);
				}
				break;
			case 14: //Dificil
				if (PlayerPrefs.GetInt ("puzzleCompleto" + controlUI.preselectedPuzzle, 0) < 4) {
					PlayerPrefs.SetInt ("puzzleCompleto" + controlUI.preselectedPuzzle, 4);
				}
				break;
			case 16: //Epico
				if (PlayerPrefs.GetInt ("puzzleCompleto" + controlUI.preselectedPuzzle, 0) < 5) {
					PlayerPrefs.SetInt ("puzzleCompleto" + controlUI.preselectedPuzzle, 5);
				}
				break;
			}
			controlUI.completeSFX.Play ();
			controlUI.ActivarPanelCompleto ();
		}
	}

	void GroupUpPieces(int k, int i, string tieneLado){
		int kk = k;
		int ii = i;
		if (tieneLado == "Left") {
			kk = kk - 1;
		} else if (tieneLado == "Right") {
			kk = kk + 1;
		} else if (tieneLado == "Above") {
			ii = ii - 1;
		} else if (tieneLado == "Below") {
			ii = ii + 1;
		}


			//Si AMBOS tienen grupos diferentes
			if (piecesArray [k, i].parent.name == "GrupoPiezas"
				&& piecesArray [kk, ii].parent.name == "GrupoPiezas"
				&& piecesArray [k, i].parent != piecesArray [kk, ii].parent) {

				Transform oldParent = piecesArray [kk, ii].parent;
				Transform[] childsToReorganize = oldParent.GetComponentsInChildren<Transform> ();
				for (int j = 0; j < childsToReorganize.Length; j++) {
					if (childsToReorganize [j].name != "Shadow") {
						childsToReorganize [j].SetParent (piecesArray [k, i].parent);
					}
				}
			if(!controlUI.fixGrupoSFX.isPlaying && !controlUI.fixFondoSFX.isPlaying) controlUI.fixGrupoSFX.Play ();
				Destroy (oldParent.gameObject);
			} 

			//Si solo ESTE tiene grupo
			else if (piecesArray [k, i].parent.name == "GrupoPiezas"
				&& piecesArray [kk, ii].parent.name != "GrupoPiezas") {

				piecesArray [kk, ii].SetParent (piecesArray [k, i].parent);
			if(!controlUI.fixGrupoSFX.isPlaying && !controlUI.fixFondoSFX.isPlaying) controlUI.fixGrupoSFX.Play ();
			} 
			//Si solo el OTRO tiene grupo
			else if (piecesArray [k, i].parent.name != "GrupoPiezas"
				&& piecesArray [kk, ii].parent.name == "GrupoPiezas") {

				piecesArray [k, i].SetParent (piecesArray [kk, ii].parent);
			if(!controlUI.fixGrupoSFX.isPlaying && !controlUI.fixFondoSFX.isPlaying) controlUI.fixGrupoSFX.Play ();
			} 
			//Si NINGUNO tiene grupo
			else if (piecesArray [k, i].parent.name != "GrupoPiezas"
				&& piecesArray [kk, ii].parent.name != "GrupoPiezas"
				&& piecesArray[k,i].tag != "PiezaColocada"
				&& piecesArray[kk,ii].tag != "PiezaColocada") {

				GameObject newParent = new GameObject();
				piecesArray [k, i].SetParent (newParent.transform);
				piecesArray [kk, ii].SetParent (newParent.transform);
				newParent.name = "GrupoPiezas";
				newParent.transform.SetParent (this.transform);
			if(!controlUI.fixGrupoSFX.isPlaying && !controlUI.fixFondoSFX.isPlaying) controlUI.fixGrupoSFX.Play ();
			}
	}
}
