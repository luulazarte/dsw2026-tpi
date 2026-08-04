##### Sistema de Turnos Médicos — Backend


##### TPI de Desarrollo de Software 2026 — UTN FRT.


##### API REST para gestionar turnos médicos: médicos, especialidades, disponibilidades y reserva/cancelación de turnos. 

##### Integrantes


##### Arias Thillois, Nazarena — 58280 - 3K4

##### Ávila, Mauro Nicolás — 58209 - 3K4

##### Lazarte, Luciana — 58199 - 3K4

##### Romero Scro, Martina — 58090 - 3K2


##### Cómo ejecutarlo


##### 1. Clonar el repositorio y pararse en la rama "development":

##### git clone https://github.com/luulazarte/dsw2026-tpi.git

##### cd dsw2026-tpi

##### git checkout development


##### 2. Abrir "Dsw2026Tpi.slnx" en Visual Studio, poner "Dsw2026Tpi.Api" como proyecto de inicio y ejecuta

##### Swagger nos lleva a "https://localhost:5278/swagger".


##### Admin inicial: "admin@system.com" / "Admin1234"


##### Autenticación


##### El sistema maneja dos roles: Administrador y Paciente. Cuando cualquiera de los dos inicia sesión, se genera un token JWT. Ese token es la llave para los endpoints que tenga permitido cada rol y hay que mandarlo en el header (Authorization: Bearer <token>).

##### Endpoints

##### **Auth** (/api/auth)

##### POST /admin/login — login admin (email + password).

##### POST /patient/login — login paciente (email + dni); si no existe, lo crea.

##### POST /admin/register — registro de admin.


##### **Especialidades** (/api/specialties) — solo admin

##### GET / — lista paginada (pageSize, pageIndex, name).

##### POST / — crear.

##### PUT /{id} — actualizar.

##### DELETE /{id} — borrado lógico.


##### **Médicos** (/api/doctors) — solo admin

##### GET / — lista paginada (pageSize, pageIndex, name).

##### GET /{id}/availabilities — disponibilidad del mes del médico.

##### POST /— crear (la especialidad tiene que existir).

##### PUT /{id} — actualizar.

##### DELETE /{id} — borrado lógico.


##### **Disponibilidades** (/api/availabilities) — solo admin

##### POST / — genera los turnos de 30 min del mes (saltea feriados).

##### PUT / — reemplaza toda la disponibilidad del mes.



##### **Turnos** (/api/appointments)

##### POST / — reservar turno (paciente).

##### GET /patient?dni= — turnos activos del paciente (paciente).

##### DELETE /{id} — cancelar turno (paciente).

##### GET /?date= — turnos de un día, paginado (admin).

##### GET /search — búsqueda avanzada por especialidad, médico, dni o fecha (admin).


##### Los turnos son bloques de 30 minutos y no se puede reservar dos veces el mismo.

##### Los feriados se cargan desde `Dsw2026Tpi.Data/Sources/feriados.json`.


##### Pruebas


##### Las pruebas unitarias (xUnit + NSubstitute) están en `Dsw2026Tpi.Tests`. 

