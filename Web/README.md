This is how .env file shoud look like

## Google Cloud Project ID
GEMINI_PROJECT_ID="your-gcp-project-id"

## Google Cloud Location for AI Platform (e.g., us-central1)
GEMINI_LOCATION="us-central1"

## Google Cloud Publisher for the model (usually "google")
GEMINI_PUBLISHER="google"

## Gemini Model Version (e.g., gemini-1.5-flash-001)
MODEL_VERSION_GEMINI="gemini-1.5-flash-001"

### Comment
For Google Cloud authentication, set the GOOGLE_APPLICATION_CREDENTIALS environment variable
to the path of your service account key file (e.g., /app/service-account-key.json).
This is typically handled outside the .env file in production environments,
but can be set here for local development or in Docker.
GOOGLE_APPLICATION_CREDENTIALS="/path/to/your/service-account-key.json"