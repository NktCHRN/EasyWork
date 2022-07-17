import { Component, EventEmitter, OnInit, Output, ViewChild } from '@angular/core';
import { MatDialog } from '@angular/material/dialog';
import { base64ToFile, CropperPosition, ImageCroppedEvent, ImageCropperComponent, ImageTransform } from 'ngx-image-cropper';
import { ErrorDialogComponent } from 'src/app/components/error-dialog/error-dialog.component';
import { AccountService } from 'src/app/services/account.service';
import { UserInfoService } from 'src/app/services/userinfo.service';
import { AvatarPageMode } from 'src/app/shared/user/cabinet/avatar-page-mode';

@Component({
  selector: 'app-avatar-edit',
  templateUrl: './avatar-edit.component.html',
  styleUrls: ['./avatar-edit.component.scss']
})
export class AvatarEditComponent implements OnInit {
  imageChangedEvent: any = '';
  croppedImage: any = '';
  scale = 1;
  readonly minScale: number = 1;
  readonly maxScale: number = 3;
  showCropper = false;
  containWithinAspectRatio = false;
  transform: ImageTransform = {};
  loading: boolean = false;
  success: boolean = false;
  fileName: string | undefined | null;
  @Output() changeMode = new EventEmitter<AvatarPageMode>();
  readonly allowedFileTypes: string[] = ["bmp", "gif", "ico", "jpeg", "jpg", "png", "tif", "tiff", "webp"];
  @ViewChild(ImageCropperComponent) imageCropper: ImageCropperComponent = undefined!;
  cropperPositions: CropperPosition = {
    x1: 0,
    y1: 0,
    x2: 0,
    y2: 0
  };
  width: number = 0;
  height: number = 0;
  
  constructor(private _dialog: MatDialog, private _accountService: AccountService, private _userInfoService: UserInfoService) { }

  ngOnInit(): void {  }

  fileChangeEvent(event: any): void {
    this.fileName = event.target.files[0].name;
    let fileType = this.fileName!.split('.').pop();
    if (!this.allowedFileTypes.includes(fileType!))
      this._dialog.open(ErrorDialogComponent, {
        panelClass: "mini-dialog-responsive",
        data: `Not supported image type: ${fileType}. Supported file types: ${this.allowedFileTypes.join(', ')}.`
      });
    else {
      this.scale = 1;
      this.imageChangedEvent = event;
      this.transform = {
        ...this.transform,
        scale: this.scale
      };
    }
  }

imageCropped(event: ImageCroppedEvent) {
    this.croppedImage = event.base64;
    this.width = event.width;
    this.height = event.height;
}

imageLoaded() {
    this.showCropper = true;
}

loadImageFailed() {
    console.log('Load failed');
    this._dialog.open(ErrorDialogComponent, {
      panelClass: "mini-dialog-responsive",
      data: "Image load failed"
    });
}

  zoomOut() {
    if (this.scale - .1 >= this.minScale)
    {
    this.scale -= .1;
    this.transform = {
        ...this.transform,
        scale: this.scale
    };
  }
}

zoomIn() {
  if (this.scale + .1 <= this.maxScale + 0.1)
  {
    this.scale += .1;
    this.transform = {
        ...this.transform,
        scale: this.scale
    };
  }
}

changeToShow(): void {
  this.changeMode.emit(AvatarPageMode.Show);
}

fixSizes() : void {
  if (this.width != this.height)
  {
    if (this.width > this.height)
    {
      let coefficient = (this.cropperPositions.x2 - this.cropperPositions.x1) / this.width;
      let difference = this.width - this.height;
      this.cropperPositions.x1 += difference * coefficient;
    }
    else
    {
      let coefficient = (this.cropperPositions.y2 - this.cropperPositions.y1) / this.height;
      let difference = this.height - this.width;
      this.cropperPositions.y1 += difference * coefficient;
    }
    this.imageCropper.crop()!;
  }
}

onSubmit()
{
  this.loading = true;
  this.fixSizes();
  let blob = base64ToFile(this.croppedImage);
  let file = new File([blob], this.fileName!);
  let formData = new FormData();
  formData.append('file', file);
  this._accountService.updateAvatar(localStorage.getItem('jwt')!, formData)
  .subscribe({
    next: () => {
      this._userInfoService.updateLastUser();
      this.loading = false;
      this.success = true;
    },
    error: error => {
      this.loading = false;
      let errorMessage = error.error ?? error.message ?? JSON.stringify(error);
      if (errorMessage == "The max length of the avatar is 8 MB")
        errorMessage += ". Your file size was " + Math.ceil(file.size / 1024) + " MB";
      this._dialog.open(ErrorDialogComponent, {
        panelClass: "mini-dialog-responsive",
        data: errorMessage
      });
    }
  })
}
}
