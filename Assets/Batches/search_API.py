pip install flask

from flask import Flask, request, jsonify
from asset_search_engine import AssetSearchEngine

app = Flask(__name__)
engine = AssetSearchEngine()  # Load existing assets

@app.route('/search', methods=['GET'])
def search():
    query = request.args.get('q')
    asset_type = request.args.get('type', None)
    
    if not query:
        return jsonify({"error": "Missing query parameter ?q=..."}), 400
    
    results = engine.search(query, asset_type)
    return jsonify({"results": results})

if __name__ == "__main__":
    app.run(port=5000, debug=True)