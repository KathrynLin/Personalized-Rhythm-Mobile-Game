from flask import Flask, request, jsonify, session
from flask_sqlalchemy import SQLAlchemy
import hashlib
import json
from flask_jwt_extended import jwt_required, get_jwt_identity,JWTManager
import os
import secrets


app = Flask(__name__)
if 'JWT_SECRET_KEY' in os.environ:
    app.config['JWT_SECRET_KEY'] = os.environ['JWT_SECRET_KEY']
else:
    app.config['JWT_SECRET_KEY'] = secrets.token_urlsafe(32)

jwt = JWTManager(app)

with open("config.json") as config_file:
    config = json.load(config_file)
    
app.config['SECRET_KEY'] = config['app_secret_key']
app.config['SQLALCHEMY_DATABASE_URI'] = f"mysql+pymysql://{config['db_username']}:{config['db_password']}@{config['host']}:{config['port']}/{config['db_name']}"
db = SQLAlchemy(app)

class User(db.Model):
    __tablename__ = 'user_logins'
    id = db.Column(db.Integer, primary_key=True)
    username = db.Column(db.String(50), nullable=False, unique=True)
    password_hash = db.Column(db.String(255), nullable=False)

class UserScore(db.Model):
    __tablename__ = 'user_scores'
    id = db.Column(db.Integer, primary_key=True)
    username = db.Column(db.String(50), nullable=False)
    score = db.Column(db.Integer, nullable=False)
    world_rank = db.Column(db.Integer)


@app.route('/api/register', methods=['POST'])
def register():
    data = request.get_json()
    username = data['username']
    password = data['password']
    if User.query.filter_by(username=username).first():
        return jsonify({"message": "Invalid username"}), 401

    hashed_password = hashlib.sha256(password.encode()).hexdigest()
    new_user = User(username=username, password_hash=hashed_password)
    try:
        db.session.add(new_user)
        db.session.commit()
        return jsonify({"message": "User registered successfully"}), 201
    except Exception as e:
        return jsonify({"message": "Error: " + str(e)}), 500




@app.route('/api/delete', methods=['POST'])
def delete():
    data = request.get_json()
    id = data['id']
    user = User.query.get(id)
    if not user:
        return jsonify({"message": "User not found"}), 401

    try:
        db.session.delete(user)
        db.session.commit()
        return jsonify({"message": "User deletion successfully"}), 200
    except Exception as e:
        return jsonify({"message": "Error: " + str(e)}), 500




@app.route('/api/login', methods=['POST'])
def login():
    data = request.get_json()
    username = data['username']
    password = data['password']
    hashed_password = hashlib.sha256(password.encode()).hexdigest()
    user = User.query.filter_by(username=username).first()
    if user and hashed_password == user.password_hash:
        access_token = create_access_token(identity=username)
        return jsonify({"message": "Login successful", "access_token": access_token}), 200
    else:
        return jsonify({"message": "Invalid credentials"}), 401
   
# @app.route('/logout', methods=['POST'])
# def logout():
#     session.pop('user_id', None)
#     return jsonify({"message": "Logout successful"}), 200

@app.route('/api/score', methods=['POST'])
@jwt_required()
def submit_score():
    data = request.get_json()
    username = get_jwt_identity()
    score = data['score']

    user_score = UserScore.query.filter_by(username=username).first()

    if user_score:
        user_score.score += score
    else:
        user_score = UserScore(username=username, score=score)
        db.session.add(user_score)

    db.session.commit()

    # Update world ranks
    update_world_ranks()

    return jsonify({"message": "Score submitted successfully"}), 200

def update_world_ranks():
    scores = UserScore.query.order_by(UserScore.score.desc()).all()
    for rank, user_score in enumerate(scores, start=1):
        user_score.world_rank = rank
    db.session.commit()

@app.route('/api/rank', methods=['GET'])
def get_world_rank():
    username = request.args.get('username')
    user_score = UserScore.query.filter_by(username=username).first()
    if user_score:
        return jsonify({"username": user_score.username, "world_rank": user_score.world_rank}), 200
    else:
        return jsonify({"message": "User not found"}), 404

@app.route('/api/leaderboard', methods=['GET'])
def leaderboard():
    top_scores = UserScore.query.order_by(UserScore.score.desc()).limit(5).all()
    leaderboard_data = [
        {"username": score.username, "score": score.score, "world_rank": score.world_rank}
        for score in top_scores
    ]
    return jsonify(leaderboard_data), 200

if __name__ == '__main__':
    app.run(debug=True)

