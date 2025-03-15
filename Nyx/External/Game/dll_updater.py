import sys
import os
import shutil
import json
from PyQt6.QtWidgets import QApplication, QWidget, QVBoxLayout, QPushButton, QLabel, QFileDialog, QTextEdit
from PyQt6.QtCore import Qt

CONFIG_FILE = "config.json"

class DLLUpdater(QWidget):
    def __init__(self):
        super().__init__()
        self.last_selected_dir = self.load_last_directory()
        self.initUI()

    def initUI(self):
        self.setWindowTitle("DLL Updater")
        self.setGeometry(100, 100, 500, 300)

        layout = QVBoxLayout()

        self.label = QLabel("Update DLLs from the selected directory")
        self.label.setAlignment(Qt.AlignmentFlag.AlignCenter)
        layout.addWidget(self.label)

        self.select_button = QPushButton("Select Source Directory")
        self.select_button.clicked.connect(self.select_directory)
        layout.addWidget(self.select_button)

        self.load_last_button = QPushButton("Load Last Directory")
        self.load_last_button.clicked.connect(self.load_last_directory_and_update)
        layout.addWidget(self.load_last_button)

        self.log_output = QTextEdit()
        self.log_output.setReadOnly(True)
        layout.addWidget(self.log_output)

        self.setLayout(layout)

    def load_last_directory(self):
        if os.path.exists(CONFIG_FILE):
            try:
                with open(CONFIG_FILE, "r") as f:
                    config = json.load(f)
                    return config.get("last_directory", "")
            except Exception:
                pass
        return ""

    def save_last_directory(self, directory):
        try:
            with open(CONFIG_FILE, "w") as f:
                json.dump({"last_directory": directory}, f)
        except Exception as e:
            self.log_output.append(f"Failed to save settings: {e}")

    def select_directory(self):
        source_dir = QFileDialog.getExistingDirectory(self, "Select Source Directory", self.last_selected_dir)
        if source_dir:
            self.last_selected_dir = source_dir
            self.save_last_directory(source_dir)
            self.update_dlls(source_dir)

    def load_last_directory_and_update(self):
        if self.last_selected_dir:
            self.update_dlls(self.last_selected_dir)
        else:
            self.log_output.append("No recent directory information available.")

    def update_dlls(self, source_dir):
        current_dir = os.getcwd()
        updated_files = 0

        for file_name in os.listdir(current_dir):
            if file_name.lower().endswith('.dll'):
                src_file = os.path.join(source_dir, file_name)
                dest_file = os.path.join(current_dir, file_name)
                if os.path.isfile(src_file):
                    try:
                        shutil.copy2(src_file, dest_file)
                        self.log_output.append(f"Updated {file_name}.")
                        updated_files += 1
                    except Exception as e:
                        self.log_output.append(f"Failed to update {file_name}: {e}")

        if updated_files == 0:
            self.log_output.append("No DLLs available for update.")
        else:
            self.log_output.append(f"Updated {updated_files} DLL(s)!")

if __name__ == '__main__':
    app = QApplication(sys.argv)
    updater = DLLUpdater()
    updater.show()
    sys.exit(app.exec())
