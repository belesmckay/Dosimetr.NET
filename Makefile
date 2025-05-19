PROJECT_NAME=Dosimeter.NET
PROJECT_PATH=./$(PROJECT_NAME)
PUBLISH_DIR=./publish
INSTALL_DIR=/opt/$(PROJECT_NAME)
SERVICE_NAME=$(PROJECT_NAME).service
SERVICE_PATH=/etc/systemd/system/$(SERVICE_NAME)
CONFIG_NAME=config.cfg
ARCH=linux-arm64

# default
all: build publish install service restart

# build project
build:
	dotnet build $(PROJECT_PATH) --configuration Release

# public project
publish:
	dotnet publish $(PROJECT_PATH) -c Release -o $(PUBLISH_DIR)

publishARM: clean build
	dotnet publish $(PROJECT_PATH) -r $(ARCH) -c Release --self-contained true -o $(PUBLISH_DIR)
	tar cvzf $(PROJECT_NAME).tar.gz -C $(PUBLISH_DIR) . 
# Instalace do cílového adresáře
install:
	mkdir -p $(INSTALL_DIR)
	cp -r $(PUBLISH_DIR)/* $(INSTALL_DIR)

# Vytvoření systemd služby
service:
	@echo "[Unit]" > $(SERVICE_NAME)
	@echo "Description=$(PROJECT_NAME) Service" >> $(SERVICE_NAME)
	@echo "After=network.target" >> $(SERVICE_NAME)
	@echo "" >> $(SERVICE_NAME)
	@echo "[Service]" >> $(SERVICE_NAME)
	@echo "WorkingDirectory=$(INSTALL_DIR)" >> $(SERVICE_NAME)
	@echo "ExecStart=$(INSTALL_DIR)/$(PROJECT_NAME) -c $(INSTALL_DIR)/$(CONFIG_NAME)" >> $(SERVICE_NAME) 
	@echo "Restart=always" >> $(SERVICE_NAME)
	@echo "RestartSec=5" >> $(SERVICE_NAME)
	@echo "SyslogIdentifier=$(PROJECT_NAME)" >> $(SERVICE_NAME)
	@echo "User=root" >> $(SERVICE_NAME)
	@echo "" >> $(SERVICE_NAME)
	@echo "[Install]" >> $(SERVICE_NAME)
	@echo "WantedBy=multi-user.target" >> $(SERVICE_NAME)
	sudo mv $(SERVICE_NAME) $(SERVICE_PATH)
	sudo systemctl daemon-reexec
	sudo systemctl daemon-reload
	sudo systemctl enable $(SERVICE_NAME)

# Restart služby
restart:
	sudo systemctl restart $(SERVICE_NAME)

# Odstranění služby a instalace
clean:
	sudo systemctl stop $(SERVICE_NAME) || true
	sudo systemctl disable $(SERVICE_NAME) || true
	sudo rm -f $(SERVICE_PATH)
	sudo rm -rf $(INSTALL_DIR)
	rm -rf $(PUBLISH_DIR)
