//  The MIT License (MIT)
//	
//	Copyright (c) 2014 Sergio Roman Lara Espinosa de los Monteros
//  http://cocinemosvideojuegosenunity.blogspot.com/
//	
//	Permission is hereby granted, free of charge, to any person obtaining a copy
//	of this software and associated documentation files (the "Software"), to deal
//	in the Software without restriction, including without limitation the rights
//	to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
//	copies of the Software, and to permit persons to whom the Software is
//	furnished to do so, subject to the following conditions:
//	
//	The above copyright notice and this permission notice shall be included in all
//	copies or substantial portions of the Software.
//	
//	THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
//	IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
//	FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
//	AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
//	LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
//	OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
//	SOFTWARE.

using UnityEngine;
using System.Collections;

[AddComponentMenu("Camera/Bounce")]

/// <summary>
/// Bounce camera, dá el comportamiento de una cámara empujada por 
/// los obstáculos para no perder de vista al personaje.
/// 
/// Bounce camera, gives you the behaviour of a camera pushed by 
/// obstacles to keep track of the character.
/// </summary>
public class BounceCamera : MonoBehaviour {
	#region Component Properties

	// Muestra los Gizmos para comprender cómo funciona éste componente.
	[Tooltip("Displays the Gizmos to understand how this component works.")]
	public bool debugging = false;

	// El objetivo que tiene que perseguir.
	[Tooltip("The target that have to follow.")]
	public Transform target;

	// La velocidad de rotación de la cámara, establecida por la entrada.
	[Tooltip("Speed of rotation of the camera established by the input.")]
	public float sensitivity = 2;

	// X es el ángulo mínimo; Y es el ángulo máximo para rotar la cámara.
	[Tooltip("X is the minimum angle; Y is the maximum angle to rotate the camera.")]
	public Vector2 angle = new Vector2(-60, 30);

	// La altura que tendrá la cámara sobre el objetivo.
	[Tooltip("Height will have the camera on the target.")]
	[Range(0.1f, 3.0f)] public float height = 0.5f;

	// X es la distancia mínima; Y es la distancia máxima entre la cámara y el objetivo.
	[Tooltip("X is the minimum distance; Y is the maximum distance between camera and target.")]
	public Vector2 distance = new Vector2(0.2f, 3.5f);

	// Suavisa el movimiento de la cámara, después de rebotrs con los obtáculos.
	[Tooltip("Smoothes the movement of the camera, then rebound with obstacles.")]
	[Range(1, 60)] public int distanceDamping = 5;

	// Especifica las capas con las que puede colisionar.
	[Tooltip("Specifies the layers with them it can collide.")]
	public LayerMask obstaclesLayer;

	#endregion

	#region Class Properties

	// Para almacenar el ángulo vertical, que especifica hacia dónde rotar en el eje y.
	// To store the vertical angle, which specifies where to rotate on the y axis.
	private float _rotationVertical;

	// Almacena la distancia en la que debe la cámara ubicarse.
	// Stores the distance at which the camera should be placed.
	private float _maxDistance;

	#endregion
	
	#region Initializers

	/// <summary>
	/// Inicia el componente.
	/// 
	/// Start the component.
	/// </summary>
	public void Start() {
		_rotationVertical = 0;
		_maxDistance = distance.y;
	}

	#endregion

	#region Updaters

	/// <summary>
	/// Se actualiza después de cada método Update, 
	/// y calcula el movimiento de la cámara, 
	/// y la distancia según los obstáculos.
	/// 
	/// It is updated after every Update method, 
	/// and calculates the movement of the camera, 
	/// and the distance according to obstacles.
	/// </summary>
	public void LateUpdate() {
		// Si el juego está en pausa no continua su ejecución.
		// If the game is paused no continuous the execution.
		if (GameManager.isPaused) return;

		// Se capturan los ángulos según la entrada del jugador.
		// Angles are captured according to player input.
		float rotationHorizontal = transform.localEulerAngles.y + Input.GetAxis("Mouse X") * sensitivity;

		_rotationVertical += Input.GetAxis("Mouse Y") * sensitivity;
		_rotationVertical = Mathf.Clamp(_rotationVertical, angle.x, angle.y);

		Quaternion rotation = Quaternion.Euler(_rotationVertical, rotationHorizontal, 0);

		// Se establecen los puntos cercano y lejano por donde la cámara se podrá mover.
		// The near and far points where the camera can move are established;
		Vector3 closePoint = target.position + rotation * new Vector3(0, height * 0.5f, -distance.x);
		Vector3 farPoint = target.position + rotation * new Vector3(0, height * 0.5f, -distance.y);

		// Se obtiene la distancia entre el personaje y el punto más lejano.
		// The distance between the character and the farthest point is obtained.
		float newDistance = Vector3.Distance(target.position, farPoint);

		// Se suaviza el movimiento.
		// The motion is smoothed.
		_maxDistance = Mathf.Lerp(_maxDistance, newDistance, distanceDamping * Time.deltaTime);

		// Se crea un rayo de colision.
		// A raycasting is created.
		RaycastHit hit;

		// Se calcula la dirección del rayo.
		// The ray direction is calculated.
		Vector3 direction = farPoint - closePoint;

		// Se crea el rayo y, según sea la colisión, se guarda la nueva distancia, 
		// que está entre el punto cercano y el lejano.

		// The ray is created and, according collision, the new distance is stored, 
		// which is between the closest point and the far.
		if (Physics.Raycast(closePoint, direction, out hit, _maxDistance, obstaclesLayer)) {
			_maxDistance = hit.distance;
		}

		// Se dibuja el rayo si esta activo el modo depuración.
		// The ray is drawn if the debug mode is active.
		if (debugging) Debug.DrawRay(closePoint, direction, Color.yellow);

		// Se aplican la rotación y la posición a la cámara.
		// Rotation and position the camera applies.
		transform.rotation = rotation;
		transform.position = rotation * new Vector3(0, height, -_maxDistance) + target.position;
	}

	#endregion
}
