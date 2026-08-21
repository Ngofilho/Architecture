# Use the official RabbitMQ image with the management plugin
FROM rabbitmq:management
RUN apt-get update
RUN apt-get install -y curl

# Expose the necessary ports
# 5672 is for AMQP (client connections)
# 15672 is for the RabbitMQ Management UI
EXPOSE 4369
EXPOSE 5671
EXPOSE 5672
EXPOSE 15671
EXPOSE 15672
EXPOSE 25672

# Optional: Set default username and password for the management UI
# These can also be passed as environment variables during `docker run`
ENV RABBITMQ_DEFAULT_USER=guest
ENV RABBITMQ_DEFAULT_PASS=guest

# You can add further configurations here if needed,
# such as copying a custom rabbitmq.conf or enabled_plugins file.
# For example, to enable additional plugins:
# COPY enabled_plugins /etc/rabbitmq/enabled_plugins

ADD RabbitMQ_BackUp/rabbitmq.config /etc/rabbitmq/
ADD RabbitMQ_BackUp/definitions.json /etc/rabbitmq/

RUN chown rabbitmq:rabbitmq /etc/rabbitmq/rabbitmq.config /etc/rabbitmq/definitions.json

CMD ["rabbitmq-server"]