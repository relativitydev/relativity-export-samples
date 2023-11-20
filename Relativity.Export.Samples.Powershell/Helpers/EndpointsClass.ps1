class Endpoints {
    [String] $baseAddress

    Endpoints($workspaceId) {
        $this.baseAddress = "Relativity.REST/api/export/v1/workspaces/$workspaceId/jobs"
    }

    [String] exportJobCreateUri($jobId) {
        return $this.baseAddress + "/$jobId"
    }

    [String] exportJobStart($jobId) {
        return $this.baseAddress + "/$jobId/start"
    }

    [String] exportJobCancel($jobId) {
        return $this.baseAddress + "/$jobId/cancel"
    }

    [String] exportJobsList([int]$start, [int]$length) {
        return $this.baseAddress + "?start=$start&length=$length"
    }

    [String] exportJobGet($jobId) {
        return $this.baseAddress + "/$jobId"
    }

    [String] exportJobSettings($jobId) {
        return $this.baseAddress + "/$jobId/settings"
    }

    [String] exportJobErrors($jobId) {
        return $this.baseAddress + "/$jobId/errors"
    }

    [String] exportHealthCheck() {
        return "Relativity.REST/api/export/v1/health-check"
    }
}