
import sqlite3
import numpy as np
from sentence_transformers import SentenceTransformer
from sklearn.neighbors import NearestNeighbors

class AssetSearchEngine:
    def __init__(self, db_path='assets.db'):
        self.db_path = db_path
        self.model = SentenceTransformer("all-MiniLM-L6-v2")
        self.nn_model = None
        self.embeddings = []
        self.asset_ids = []
        self._load_assets_and_build_index()

    def _connect_db(self):
        return sqlite3.connect(self.db_path)

    def _load_assets_and_build_index(self):
        conn = self._connect_db()
        cursor = conn.cursor()
        cursor.execute("SELECT id, name, description FROM assets")
        rows = cursor.fetchall()
        conn.close()

        self.embeddings = []
        self.asset_ids = []
        for row in rows:
            id_, name, description = row
            self.asset_ids.append(id_)
            text = f"{name} {description}"
            embedding = self.model.encode(text)
            self.embeddings.append(embedding)

        if self.embeddings:
            n_samples = len(self.embeddings)
            n_neighbors = min(n_samples, 3)
            self.nn_model = NearestNeighbors(n_neighbors=n_neighbors, metric='cosine')
            self.nn_model.fit(np.array(self.embeddings))

    def index_sample_assets(self, assets):
        conn = self._connect_db()
        cursor = conn.cursor()

        cursor.execute("DROP TABLE IF EXISTS assets")
        cursor.execute("""
            CREATE TABLE assets (
                id INTEGER PRIMARY KEY AUTOINCREMENT,
                name TEXT,
                description TEXT,
                type TEXT,
                path TEXT
            )
        """)
        conn.commit()

        self.embeddings = []
        self.asset_ids = []

        for asset in assets:
            text = f"{asset['name']} {asset['description']}"
            vector = self.model.encode(text)
            self.embeddings.append(vector)
            cursor.execute("INSERT INTO assets (name, description, type, path) VALUES (?, ?, ?, ?)",
                           (asset['name'], asset['description'], asset['type'], asset['path']))
            self.asset_ids.append(cursor.lastrowid)

        conn.commit()
        conn.close()

        n_samples = len(self.embeddings)
        n_neighbors = min(n_samples, 3)
        self.nn_model = NearestNeighbors(n_neighbors=n_neighbors, metric='cosine')
        self.nn_model.fit(np.array(self.embeddings))

    def search(self, query, asset_type=None):
        if not self.nn_model:
            return []

        query_embedding = self.model.encode(query).reshape(1, -1)
        distances, indices = self.nn_model.kneighbors(query_embedding)

        conn = self._connect_db()
        cursor = conn.cursor()

        results = []
        for idx in indices[0]:
            asset_id = self.asset_ids[idx]
            cursor.execute("SELECT path, type FROM assets WHERE id=?", (asset_id,))
            row = cursor.fetchone()
            if row:
                path, type_ = row
                if asset_type is None or type_ == asset_type:
                    results.append(path)

        conn.close()
        return results


# Example Usage
if __name__ == "__main__":
    assets = [
        {"name": "Red Dragon Texture", "description": "A detailed texture of a fierce red dragon.", "type": "Texture", "path": "/assets/textures/red_dragon.png"},
        {"name": "Red Dragon Model", "description": "A 3D Model of a fierce red dragon.", "type": "3D Model", "path": "/assets/textures/red_dragon.fbx"},
        {"name": "Forest Background", "description": "A lush green forest background with trees and mist.", "type": "Image", "path": "/assets/backgrounds/forest.jpg"},
        {"name": "Magic Sword Model", "description": "A 3D model of an enchanted sword with glowing runes.", "type": "3D Model", "path": "/assets/models/magic_sword.obj"},
        {"name": "Player Character Prefab", "description": "A complete player character with animations.", "type": "Prefab", "path": "/assets/prefabs/player.prefab"},
    ]

    engine = AssetSearchEngine()
    engine.index_sample_assets(assets)

    print("Search result:", engine.search("fire dragon skin"))
    print("Search 3D:", engine.search("enchanted sword", asset_type="3D Model"))
    print("Search Image:", engine.search("misty forest", asset_type="Image"))
    print("Search:", engine.search("dragon character with sword"))
