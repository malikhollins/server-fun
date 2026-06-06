FROM libretime/icecast:2.5.0

# The image entrypoint rewrites /etc/icecast.xml from ICECAST_* env vars at
# startup, so the file must be writable by the (non-root) icecast user.
COPY --chown=icecast icecast.xml /etc/icecast.xml
