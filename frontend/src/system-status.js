(function () {
  const api = window.OmniStayApi;
  const ui = window.OmniStaySession;
  const session = ui.requireAuth();
  if (!session) {
    return;
  }

  ui.hydrateNav('status');

  const target = document.getElementById('statusPanel');
  const refreshButton = document.getElementById('refreshStatus');

  refreshButton.addEventListener('click', loadStatus);

  async function loadStatus() {
    ui.renderMessage(target, 'Dang kiem tra runtime...', 'muted');

    try {
      const status = await api.getAwsStatus();
      target.className = 'status-grid';
      target.hidden = false;
      target.innerHTML = [
        ['Environment', status.environment],
        ['Region', status.region],
        ['Database', status.databaseProvider],
        ['Redis configured', status.redisConfigured ? 'Yes' : 'No'],
        ['Redis connected', status.redisConnected ? 'Yes' : 'No'],
        ['Cache TTL', `${status.searchCacheTtlSeconds}s`],
        ['S3 bucket', status.s3FrontendBucket],
        ['CloudFront', status.cloudFrontDomain],
        ['API base path', status.apiBasePath]
      ].map(([label, value]) => `
        <article class="status-item">
          <span>${ui.escapeHtml(label)}</span>
          <strong>${ui.escapeHtml(value)}</strong>
        </article>
      `).join('');
    } catch (error) {
      ui.renderMessage(target, error.message, 'error');
    }
  }

  loadStatus();
})();
