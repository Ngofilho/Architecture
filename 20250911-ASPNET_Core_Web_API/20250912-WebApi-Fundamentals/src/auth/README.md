# Authenticator Application

### How to Run

#### Docker
- `docker build . --tag authenticator:1.0`   
- `docker run -p 7172:8080` --name Auth authenticator:1.0   

#### Visual Studio
- `F5`

#### To consume
**GET**:
- http://localhost:7172/auth?user=Kevin&password=verdao   
- http://localhost:7172/authenticate?user=Kevin&password=verdao   
- http://localhost:7172/authentication?user=Kevin&password=verdao   

**POST**:
- http://localhost:7172/auth?user=Kevin&password=verdao   
- http://localhost:7172/authenticate?user=Kevin&password=verdao   
- http://localhost:7172/authentication?user=Kevin&password=verdao   

```json
{
	"Username":"Kevin", "Password":"verdao"
}
```