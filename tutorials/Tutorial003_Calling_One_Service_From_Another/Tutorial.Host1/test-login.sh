# Run this to test the `CreateUser` method

# {
#   "operationName": "Login",
# 	"payloadObj": {
# 		"username": "kermit",
#       "password": "ribbot!"
# 	}
# }

curl -d '{"operationName":"Login","payloadObj":{"username":"kermit","password":"ribbot!"}}' -H "Content-Type: application/json" -X POST http://localhost:5000/managed/Tutorial/Session/1

# {
#     "payload": null,
#     "payloadObj": null,
#     "serviceCallStatus": 1,
#     "operationStatus": 1,
#     "serviceStatus": {
#         "instanceId": "835511b8c88a46f2ad6b453933670a07",
#         "availability": 5,
#         "health": 4,
#         "runState": 3
#     },
#     "message": null,
#     "responderInstanceId": "835511b8c88a46f2ad6b453933670a07",
#     "responderFabricId": "361e06ed43fe4ac6b33e5cff614b8243",
#     "operationId": "fe267c70-fcd6-47ee-b2f7-fd709538987c",
#     "operationName": "CreateUser",
#     "service": {
#         "isMetaService": false,
#         "collection": "Tutorial",
#         "name": "User",
#         "version": 1,
#         "updateLevel": 0,
#         "patchLevel": 0
#     },
#     "correlationId": "0394b857f4214788988bf38bb871d31b",
#     "requestorInstanceId": null,
#     "requestorFabricId": null,
#     "code": null,
#     "hasError": false,
#     "immediateSuccess": true,
#     "completed": true
# }
