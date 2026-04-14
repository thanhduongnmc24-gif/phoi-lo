

git add . && git commit -m "Cập nhật" && git push origin main

tạo thư mục out để lưu file exe
    dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:EnableWindowsTargeting=true -o out

# Di chuyển ra thư mục gốc của dự án cho chắc cú
cd /workspaces/phoi-lo

# Bắt Git "quên" các thư mục chứa file build nặng đi (chỉ quên trên Git chứ không xóa file thật)
git rm -r --cached PhoiLo/out/
git rm -r --cached PhoiLo/bin/
git rm -r --cached PhoiLo/obj/

# Ghi luật vào file .gitignore để từ nay về sau Git tự động phớt lờ mấy thư mục này
echo "PhoiLo/out/" >> .gitignore
echo "PhoiLo/bin/" >> .gitignore
echo "PhoiLo/obj/" >> .gitignore

# Cập nhật lại cái commit bị lỗi lúc nãy, chính thức vứt cục .exe ra khỏi kiện hàng
git commit --amend --no-edit

# Đẩy mã nguồn nhẹ nhàng lên mây
git push origin main