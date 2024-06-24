for broj in {1..100}; do
	curl "http://localhost:8080/analyze?owner=xournalpp&repository=xournalpp&issueNumber=2000"
done
