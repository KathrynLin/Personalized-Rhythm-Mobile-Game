#!/bin/bash

# Configurations
BASE_URL="http://127.0.0.1:5000/api"
USERNAME="testuser"
PASSWORD="testpassword"
SCORE=1500

# Register a new user
echo "Registering user..."
REGISTER_RESPONSE=$(curl -s -X POST -H "Content-Type: application/json" -d '{
    "username": "'"${USERNAME}"'",
    "password": "'"${PASSWORD}"'"
}' "${BASE_URL}/register")

echo "Register response: $REGISTER_RESPONSE"

# Log in the user
echo "Logging in user..."
LOGIN_RESPONSE=$(curl -s -X POST -H "Content-Type: application/json" -d '{
    "username": "'"${USERNAME}"'",
    "password": "'"${PASSWORD}"'"
}' "${BASE_URL}/login")

echo "Login response: $LOGIN_RESPONSE"

# Submit a score
echo "Submitting score..."
SUBMIT_SCORE_RESPONSE=$(curl -s -X POST -H "Content-Type: application/json" -d '{
    "username": "'"${USERNAME}"'",
    "score": '"${SCORE}"'
}' "${BASE_URL}/score")

echo "Submit score response: $SUBMIT_SCORE_RESPONSE"

# Get world rank
echo "Getting world rank..."
WORLD_RANK_RESPONSE=$(curl -s -X GET "${BASE_URL}/rank?username=${USERNAME}")

echo "World rank response: $WORLD_RANK_RESPONSE"

