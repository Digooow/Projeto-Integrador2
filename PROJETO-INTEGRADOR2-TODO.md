# Projeto-Integrador2 — Actionable TODOs

Generated: 2026-09-05  
Source: repo inspection and CI/docs review

## Quick verdict
Maturity: Solid Intermediate — near production but blocked by auth, RLS validation, and a few security/ops items.

---

## Prioritized Next Actions (High → Low)
- [ ] 1. Fix frontend JWT integration and complete end‑to‑end auth flow
  - Ensure frontend stores/sends Bearer token securely (session/local storage with appropriate protections).
  - Add unit/E2E tests for login, token usage, protected endpoints.
  - Estimate: 4–8 hours.

- [ ] 2. Validate & harden Row Level Security (RLS) on Supabase
  - Run migration 002 in a staging Supabase instance and validate INSERT/UPDATE/DELETE policies.
  - Add CI smoke test to verify RLS behaviors.
  - Estimate: 2–6 hours.

- [ ] 3. Configure production CORS and rate limiting
  - Restrict allowed origins and tighten headers.
  - Add rate-limiting middleware or reverse-proxy rules.
  - Estimate: 1–3 hours.

- [ ] 4. Add security scanning to CI
  - Add Trivy (container) or other image scanner + Dependabot and CodeQL.
  - Fail the pipeline on CRITICAL/HIGH severities by policy.
  - Estimate: 1–3 hours.

- [ ] 5. Publish test coverage and add a badge
  - Integrate coverlet + Codecov (or other) and display coverage badge in README.
  - Estimate: 1–2 hours.

- [ ] 6. Improve CI secrets handling & document required secrets
  - Use least-privilege tokens for Docker Hub and Render; document rotation policy.
  - Estimate: 1–2 hours.

- [ ] 7. Add CODEOWNERS, CONTRIBUTING.md and issue templates
  - Improves collaboration and review flow.
  - Estimate: 30–60 minutes.

- [ ] 8. Fix README placeholders and add a Quick Start script
  - Update links to real repo/service URLs and add a single-command local startup script.
  - Estimate: 30–60 minutes.

---

## Suggested Quick Improvements (can be implemented fast)
- [ ] Add a Trivy CI job to scan pushed images.
- [ ] Add a coverage upload step to the pipeline and show badge.
- [ ] Add a minimal xUnit test that asserts token issuance + a protected endpoint.
- [ ] Add Dependabot config for dependency updates.

---

## Example: Trivy CI snippet (copy into `.github/workflows/dotnet.yml`)
```yaml
# job: security-scan (after docker-build-push)
security-scan:
  needs: docker-build-push
  runs-on: ubuntu-latest
  steps:
    - name: Pull built image
      run: docker pull ${{ env.REGISTRY }}/${{ secrets.DOCKER_USERNAME }}/${{ env.IMAGE_NAME }}:latest

    - name: Install Trivy
      run: |
        curl -sSfL https://raw.githubusercontent.com/aquasecurity/trivy/main/contrib/install.sh | sudo sh -s -- -b /usr/local/bin

    - name: Scan image with Trivy (fail on HIGH/CRITICAL)
      run: trivy image --exit-code 1 --severity CRITICAL,HIGH ${{ env.REGISTRY }}/${{ secrets.DOCKER_USERNAME }}/${{ env.IMAGE_NAME }}:latest