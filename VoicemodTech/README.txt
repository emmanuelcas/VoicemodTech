El proyecto esta subido como proyecto de libreria de clases, para su facilidad a la hora de la prueba pueden abrir el proyecto
y simplemente en las propiedades de la aplicacion le cambian la salida a tipo de consola

Para realizar mis pruebas simplemente lo que hice fue desarrollarlo como app de consola, y abrir varios exe
en el primero ingresaba el numero de puerto, y ese quedaba establecido como el "server", mientras los demas como clientes
Basicamente mi solucion busca si esta iniciada una sala de chat te unis y chateas, y si sos el primero en ese puerto vas a ser
el "server", y cuando se una mas gente podes chatear con ellos. Los demas se unen como diferentes clientes

Cuando alguien se une o deja el server, todos son notificados de esto
Cuando el que chatea y hace de server decide dejar de chatear, a los demas les llegara un mensaje qeu dice que se ha cerrado la sala de chat
Para irse/cerrar el programa se debe escribir la palabra "exit"

Se genero todo en una clase unica por la simplicidad de la solucion para cumplir con lo reqeurido (considero que complijizar la solucion
con otro tipo de arquitecturas y diseños seria algo innecesario)
