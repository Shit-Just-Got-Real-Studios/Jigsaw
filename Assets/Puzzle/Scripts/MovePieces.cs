using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovePieces : MonoBehaviour {

	bool isSelected;

	float rayDistance; //Calculated when we click
	GameObject clickedPiece; //The piece we've just clicked and we want to move
	Transform[] movedPieces;
	[HideInInspector] public AttachPieces attachPieces;
	[HideInInspector] public float zPos = -0.001f;
	[HideInInspector] public bool isPuzzlePlaying;
	Vector3 offset;

	void Update () {
		//PULSAR RATON
		if (isPuzzlePlaying) {
			if (Input.GetMouseButtonDown(0)) {
				PositionClick ();
			}

			//MANTENER RATON
			if (isSelected) {
				PieceMovement ();
			}

			//SOLTAR RATON
			if (Input.GetMouseButtonUp (0)) {
				ReleasePiece ();
			}
		}
	}

	void PositionClick(){
		Ray rayo = Camera.main.ScreenPointToRay(Input.mousePosition); 	//Rayo que va desde la camara hasta el punto
		RaycastHit hit;
		if (Physics.Raycast (rayo, out hit)) {							//Rayo y hayamos el hit si es que hay
			if (hit.collider.gameObject.tag == "PiezaPuzzle") {
				zPos -= 0.01f;
				rayDistance = hit.distance;
				clickedPiece = hit.collider.gameObject;
				offset = hit.point-hit.collider.gameObject.transform.position;
				offset.z = 0;
                hit.collider.transform.GetChild (0).GetComponent<MeshRenderer> ().enabled = false; //Desactivamos la sombra
				if (clickedPiece.transform.parent.name == "GrupoPiezas") {
					BoxCollider[]hijosARecolocarBox = clickedPiece.transform.parent.GetComponentsInChildren<BoxCollider> ();
					Transform[] hijosARecolocar = new Transform[hijosARecolocarBox.Length];
					for (int i = 0; i < hijosARecolocar.Length; i++) {
						hijosARecolocar[i] = hijosARecolocarBox [i].transform;
                        hijosARecolocar [i].GetChild (0).GetComponent<MeshRenderer> ().enabled = false; //Desactivamos las sombras
					}

					movedPieces = new Transform[hijosARecolocar.Length];
					for (int i = 0; i < movedPieces.Length; i++) {
						movedPieces [i] = hijosARecolocar [i];
					}
					hijosARecolocar = movedPieces; //Estas 4 lineas para evitar que se coja al padre como [0]

					Vector3[] posicionRelativa = new Vector3[hijosARecolocar.Length];
					GameObject nuevoParent = clickedPiece.transform.parent.gameObject;

					for (int i = 0; i < hijosARecolocar.Length; i++) {
						if (hijosARecolocar [i].tag == "PiezaPuzzle" && hijosARecolocar [i] != clickedPiece.transform) {
							posicionRelativa [i] = hijosARecolocar [i].position - clickedPiece.transform.position;
							hijosARecolocar [i].transform.parent = nuevoParent.transform.parent;
						}
					}

					nuevoParent.transform.position = Input.mousePosition;

					for (int i = 0; i < hijosARecolocar.Length; i++) {
						if (hijosARecolocar [i].tag == "PiezaPuzzle") {
							hijosARecolocar [i].transform.parent = nuevoParent.transform;
                            hijosARecolocar [i].localPosition = posicionRelativa [i];
						}
					}

					clickedPiece = nuevoParent;
				} else {
					movedPieces = new Transform[1];
					movedPieces [0] = clickedPiece.transform;
				}
				isSelected = true;
			}
		}
	}

	void PieceMovement(){
		Ray ray = Camera.main.ScreenPointToRay (Input.mousePosition);
		Vector3 rayLimit = ray.GetPoint (rayDistance);
		rayLimit = new Vector3 (rayLimit.x, rayLimit.y, zPos);
		clickedPiece.transform.position = rayLimit - offset;
	}

	void ReleasePiece(){
		if(isSelected){
			clickedPiece.transform.position = new Vector3 (clickedPiece.transform.position.x, clickedPiece.transform.position.y, zPos);


			for (int i = 0; i < movedPieces.Length; i++) {
				if(movedPieces [i].tag != "PiezaColocada"){
					movedPieces [i].GetChild (0).GetComponent<MeshRenderer> ().enabled = true; //Reactivamos las sombras
				}
				string pieceNum = movedPieces[i].gameObject.name.Substring (6, movedPieces[i].gameObject.name.Length-6);
				string[] numbOfPieces = pieceNum.Split('x');
				attachPieces.ReadjustPieces (int.Parse(numbOfPieces [1]), int.Parse(numbOfPieces [0])); //Las piezas estan numeradas al reves, primero el orden vertical, luego el horizontal
			}

			if (clickedPiece.name == "GrupoPiezas") {
				foreach (Transform childPiece in clickedPiece.transform) {
					childPiece.position = new Vector3 (childPiece.position.x, childPiece.position.y, zPos);
				}
			}

			clickedPiece = null;
			movedPieces = new Transform[0];
			isSelected = false;
		}
	}
}
