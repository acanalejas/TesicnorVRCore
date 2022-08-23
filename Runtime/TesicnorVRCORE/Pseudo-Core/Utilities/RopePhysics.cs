using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class RopePhysics 
{
    #region PARAMETERS
    /// <summary>
    /// Aceleración de la gravedad
    /// </summary>
    public static float gravity = 9.81f;
    #endregion
    ///Para las operaciones a continuacion asumiremos lo siguiente sobre las secciones de la cuerda
    ///-Posicion 0 -> Seccion superior
    ///-Posicion max -> Seccion inferior
    #region FUNCTIONS

    public static Vector3 gravityForce(float _mass)
    {
        return Vector3.down * (_mass * gravity);
    }

    /// <summary>
    /// Fuerza elástica de la cuerda
    /// </summary>
    /// <param name="_coeficent">Coeficiente de elasticidad</param>
    /// <param name="_positionDiference">Diferencia de longitud entre la deseada y la actual</param>
    /// <param name="_direction">Dirección de la fuerza</param>
    /// <returns></returns>
    public static Vector3 elasticForce(float _coeficent, float _positionDiference, float _previousDiference, Vector3 _previousDirection, Vector3 _direction)
    {
        Vector3 thisForce = _direction.normalized * (_coeficent * _positionDiference);
        Vector3 previousForce = _previousDirection.normalized * (_coeficent * _previousDiference);
        //thisForce -= previousForce;

        return thisForce;
    }

    /// <summary>
    /// Devuelve la fuerza de rozamiento con el aire
    /// Fd = c * v^2
    /// </summary>
    /// <param name="_coeficent">Coeficiente de rozamiento con el aire</param>
    /// <param name="_velocity">Magnitud de la velocidad</param>
    /// <param name="_direction">Dirección de la fuerza</param>
    /// <returns></returns>
    public static Vector3 dampForce(float _coeficent, float _velocity, Vector3 _direction)
    {
        return _direction * (_coeficent* (_velocity*_velocity));
    }

    /// <summary>
    /// Devuelve la fuerza externa que se ejecuta sobre la cuerda cuando está siendo movida por estar agarrada
    /// </summary>
    /// <param name="_velocity">velocidad en el frame que se calcula</param>
    /// <param name="_previousVelocity">velocidad en el frame anterior al calculo</param>
    /// <param name="_mass">masa del objeto agarrado</param>
    /// <param name="_time">tiempo de la simulacion</param>
    /// <param name="_direction">direccion de la fuerza resultante</param>
    /// <returns></returns>
    public static Vector3 externalForceGrab(float _velocity, float _previousVelocity, float _mass, float _time, Vector3 _direction)
    {
        float acceleration = (_velocity - _previousVelocity) / _time;

        return _direction*(_mass * acceleration);
    }

    /// <summary>
    /// Devuelve la fuerza que se produce en una colision
    /// </summary>
    /// <param name="_velocity">velocidad en el frame que se simula</param>
    /// <param name="_mass">masa del objeto</param>
    /// <param name="_percentage">porcentaje de velocidad PERDIDA en la colision</param>
    /// <param name="_time">tiempo de la simulacion</param>
    /// <param name="_direction">direccion de la fuerza resultante</param>
    /// <returns></returns>
    public static Vector3 collisionForce(float _velocity, float _mass, float _percentage, float _time, Vector3 _direction)
    {
        float newVelocity = _velocity * _percentage;
        float acceleration = (newVelocity - _velocity) / _time;

        return _direction*(acceleration * _mass);
    }

    /// <summary>
    /// Devuelve la fuerza normal que se genera cuando la cuerda esta sobre una superficie
    /// </summary>
    /// <param name="_mass">masa del objeto</param>
    /// <param name="_surfaceNormal">vector normal de la superficie sobre la que está</param>
    /// <returns></returns>
    public static Vector3 normalForce(float _mass, Vector3 _surfaceNormal)
    {
        Vector3 surfaceNormal = _surfaceNormal.normalized;
        return Vector3.up * (_mass * gravity * surfaceNormal.y);
    }

    /// <summary>
    /// Devuelve la fuerza centrípeta de la sección de la cuerda
    /// Se podría decir que esta rota desde su sección superior
    /// </summary>
    /// <param name="_mass">masa del objeto</param>
    /// <param name="_velocity">velocidad del objeto</param>
    /// <param name="_radius">radio del giro (longitud de la seccion)</param>
    /// <param name="_direction">direccion de la fuerza resultante</param>
    /// <returns></returns>
    public static Vector3 centripetalForce(float _mass, float _velocity, float _radius, Vector3 _direction)
    {
        return _direction * ((_mass * (_velocity * _velocity)) / _radius);
    }

    /// <summary>
    /// Devuelve el sumatorio de fuerzas
    /// </summary>
    /// <param name="_mass">masa del objeto</param>
    /// <param name="_k">constante elastica</param>
    /// <param name="_positionDiference">diferencia entre la longitud deseada y actual de la seccion</param>
    /// <param name="_kDirection">direccion de la fuerza elastica</param>
    /// <param name="_d">constante de rozamiento del aire</param>
    /// <param name="_velocity">velocidad del objeto</param>
    /// <param name="_dDirection">direccion de la fuerza de rozamiento</param>
    /// <param name="_time">tiempo de la simulacion</param>
    /// <param name="isGrabbed">el objeto esta siendo agarrado?</param>
    /// <param name="_previousVelocity">velocidad del frame anterior en la simulacion</param>
    /// <param name="_handDirection">direccion hacia la que se mueve la mano (hand.velocity.normalized)</param>
    /// <param name="isInCollision">esta el objeto en colision?</param>
    /// <param name="_percentage">porcentage de velocidad PERDIDA en colision</param>
    /// <param name="_collisionDirection">direccion de la fuerza resultante de la colision</param>
    /// <param name="_surfaceNormal">normal de la superficie con la que se colisiona</param>
    /// <param name="_radius">longitud del segmento</param>
    /// <param name="_centripetalDirection">direccion de la fuerza centripeta</param>
    /// <returns></returns>
    public static Vector3 allForces(float _mass, float _k, float _positionDiference, Vector3 _kDirection, float _d, float _velocity, Vector3 _dDirection, float _time, bool isGrabbed, float _previousVelocity, Vector3 _handDirection, bool isInCollision, float _percentage, Vector3 _collisionDirection, Vector3 _surfaceNormal, float _radius, Vector3 _centripetalDirection)
    {
        //Vector3 result = gravityForce(_mass) + elasticForce(_k, _positionDiference, _kDirection) + dampForce(_d, _velocity, _dDirection) + centripetalForce(_mass, _velocity, _radius, _centripetalDirection);

        //if (isGrabbed) result += externalForceGrab(_velocity, _previousVelocity, _mass, _time, _handDirection);
        //if (isInCollision) result += collisionForce(_velocity, _mass, _percentage, _time, _collisionDirection) + normalForce(_mass, _surfaceNormal);

        return Vector3.zero;
    }
    #endregion
}
