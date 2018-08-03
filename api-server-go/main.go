package main

import (
	"encoding/json"
	"fmt"
	"log"
	"net/http"
	"os"
	"time"

	"cloud.google.com/go/spanner"
	"github.com/satori/go.uuid"
	"golang.org/x/net/context"
)

var (
	projectID = os.Getenv("PROJECT_ID")
	ctx       = context.Background()
)

func main() {

	log.Print("API Server starting up.")

	http.HandleFunc("/trigger", triggerHandler)
	http.HandleFunc("/statusReport", statusReportHandler)
	http.HandleFunc("/results", resultHandler)
	http.ListenAndServe(":8080", nil)
}

type triggerRequest1 struct {
	AccountID int    `json:"accountId"`
	SourceURL string `json:"sourceUrl"`
}

type apiErrorResponse struct {
	ErrorCode int    `json:"errorCode"`
	Message   string `json:"message"`
}

func triggerHandler(w http.ResponseWriter, r *http.Request) {

	// read and validate the request payload
	decoder := json.NewDecoder(r.Body)

	var t triggerRequest1
	err := decoder.Decode(&t)
	if err != nil {
		writeErrorResponse(w, "decoding JSON", 101, err)
		return
	}

	client := getSpannerClient()

	buildID, err := uuid.NewV4()
	accountID := 1
	sourceUrl := r.URL.Query().Get("")

	buildColumns := []string{"BuildID", "AccountID", "CurrentState", "CurrentStateTimestamp", "SourceUrl", "TriggerReason"}

	buildValues := []interface{}{buildID, accountID, 1, time.Now(), sourceUrl, triggerReason}

	spanner.InsertOrUpdate("Builds", buildColumns, buildValues)
}

func writeErrorResponse(w http.ResponseWriter, message string, code int, err error) {
	log.Printf("Error %s: %s", message, err)

	w.WriteHeader(http.StatusBadRequest)
	w.Header().Set("Content-Type", "application/json")

	responseBody := &apiErrorResponse{
		ErrorCode: code,
		Message:   message}

	json.NewEncoder(w).Encode(responseBody)
}

func statusReportHandler(w http.ResponseWriter, r *http.Request) {

}

func resultHandler(w http.ResponseWriter, r *http.Request) {

}

func getSpannerClient() *spanner.Client {
	databaseName := fmt.Sprintf("projects/%s/instances/apidoctorservice/databases/builds-db", projectID)
	client, err := spanner.NewClient(ctx, databaseName)
	if err != nil {
		log.Fatalf("Failed to create client %v", err)
	}
	defer client.Close()
	return client
}
