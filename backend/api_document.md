## API Endpoints

### 1. Register User

**Endpoint:** `/api/register`  
**Method:** `POST`  
**Description:** Registers a new user with a username and password.

**Request:**

```json
{
  "username": "your_username",
  "password": "your_password"
}
```

**Response:**

- Success: `201 Created`
  ```json
  {
    "message": "User registered successfully"
  }
  ```
- Error: `401 Unauthorized` if the username already exists.
  ```json
  {
    "message": "Invalid username"
  }
  ```

### 2. Login User

**Endpoint:** `/api/login`  
**Method:** `POST`  
**Description:** Logs in a user with a username and password.

**Request:**

```json
{
  "username": "your_username",
  "password": "your_password"
}
```

**Response:**

- Success: `200 OK`
  ```json
  {
    "message": "Login successful"
  }
  ```
- Error: `401 Unauthorized` if the credentials are invalid.
  ```json
  {
    "message": "Invalid credentials"
  }
  ```

### 3. Submit Score

**Endpoint:** `/api/score`  
**Method:** `POST`  
**Description:** Submits or updates the score for a user. (The world rank is recalculated after submission.)

**Request:**

```json
{
  "username": "your_username",
  "score": 1500
}
```

**Response:**

- Success: `200 OK`
  ```json
  {
    "message": "Score submitted successfully"
  }
  ```

### 4. Get World Rank

**Endpoint:** `/api/rank`  
**Method:** `GET`  
**Description:** Retrieves the world rank for a specific user.

**Request:**

```http
GET /api/rank?username=your_username
```

**Response:**

- Success: `200 OK`
  ```json
  {
    "username": "your_username",
    "world_rank": 1
  }
  ```
- Error: `404 Not Found` if the user is not found.
  ```json
  {
    "message": "User not found"
  }
  ```

### 5. Delete User

**Endpoint:** `/api/delete`  
**Method:** `POST`  
**Description:** Deletes a user based on their ID.

**Request:**

```json
{
  "id": 1
}
```

**Response:**

- Success: `200 OK`
  ```json
  {
    "message": "User deletion successfully"
  }
  ```
- Error: `401 Unauthorized` if the user is not found.
  ```json
  {
    "message": "User not found"
  }
  ```


### 6. Leaderboard

**Endpoint:** `/api/leaderboard`  
**Method:** `GET`  
**Description:** Retrieves the top five scores along with usernames and world ranks.

**Request:** None needed. Simply make a GET request to the endpoint.

**Response:**

- Success: `200 OK`
  ```json
  [
      {
          "username": "player1",
          "score": 1200,
          "world_rank": 1
      },
      {
          "username": "player2",
          "score": 1150,
          "world_rank": 2
      },
      {
          "username": "player3",
          "score": 1100,
          "world_rank": 3
      },
      {
          "username": "player4",
          "score": 1050,
          "world_rank": 4
      },
      {
          "username": "player5",
          "score": 1000,
          "world_rank": 5
      }
  ]
  ```



## Example Usage with `curl`

### Register a User

```sh
curl -X POST http://localhost:5000/api/register -H "Content-Type: application/json" -d '{
  "username": "testuser",
  "password": "testpassword"
}'
```

### Login a User

```sh
curl -X POST http://localhost:5000/api/login -H "Content-Type: application/json" -d '{
  "username": "testuser",
  "password": "testpassword"
}'
```

### Submit a Score

```sh
curl -X POST http://localhost:5000/api/score -H "Content-Type: application/json" -d '{
  "username": "testuser",
  "score": 1500
}'
```

### Get World Rank

```sh
curl -X GET "http://localhost:5000/api/rank?username=testuser"
```

### Delete a User

```sh
curl -X POST http://localhost:5000/api/delete -H "Content-Type: application/json" -d '{
  "id": 1
}'
```

### Get Leaderboard

```sh
curl -X GET http://localhost:5000/api/leaderboard
```
