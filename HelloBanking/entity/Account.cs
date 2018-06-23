using System;
using System.Collections.Generic;
using HelloBanking.model;
using HelloBanking.utility;

namespace HelloBanking.entity
{
    public class Account
    {
        private string _accountNumber; //id
        private string _username;
        private string _password;
        private string _cpassword;
        private string _salt;
        private decimal _balance;
        private string _identityCard;
        private string _fullname;
        private string _email;
        private string _phoneNumber;
        private string _address;
        private string _dob;
        private int _gender;
        private string _createdAt;
        private string _updatedAt;
        private int _status;
        
        //Tham số là chuỗi password chưa mã hóa mà người dùng nhập vào
        public bool CheckEncryPassword(string password)
        {
            // Tiến hành mã hóa người dùng nhập vào kèm theo muối được lấy từ database
            // Trả về một chuối password đã được mã hóa 
            var checkPassword = Hash.EncrytedString(password, _salt);
            //So sánh hai chuỗi password đã mã hóa.Nếu trùng nhau thì trả về true
            //Nếu không trùng nhau trả về false
            return (checkPassword == _password);
        }

        public void EncryptPassword()
        {
            if (string.IsNullOrEmpty(_password))
            {
                throw new ArgumentNullException("Password is null or empyt");
            }

            _password = Hash.EncrytedString(_password, _salt);
        }

        private void GenerateAccountNumber()
        {
            _accountNumber = Guid.NewGuid().ToString();
        }

        private void GenerateSalt()
        {
            _salt = Guid.NewGuid().ToString().Substring(0, 7);
        }

        public string AccountNumber
        {
            get => _accountNumber;
            set => _accountNumber = value;
        }

        public string Username
        {
            get => _username;
            set => _username = value;
        }

        public string Password
        {
            get => _password;
            set => _password = value;
        }

        public string Cpassword
        {
            get => _cpassword;
            set => _cpassword = value;
        }

        public string Salt
        {
            get => _salt;
            set => _salt = value;
        }

       

        public string IdentityCard
        {
            get => _identityCard;
            set => _identityCard = value;
        }

        public string Fullname
        {
            get => _fullname;
            set => _fullname = value;
        }

        public string Email
        {
            get => _email;
            set => _email = value;
        }

        public string PhoneNumber
        {
            get => _phoneNumber;
            set => _phoneNumber = value;
        }

        public string Address
        {
            get => _address;
            set => _address = value;
        }

        public string Dob
        {
            get => _dob;
            set => _dob = value;
        }


        public decimal Balance
        {
            get => _balance;
            set => _balance = value;
        }

        public int Gender
        {
            get => _gender;
            set => _gender = value;
        }

        public string CreatedAt
        {
            get => _createdAt;
            set => _createdAt = value;
        }

        public string UpdatedAt
        {
            get => _updatedAt;
            set => _updatedAt = value;
        }

        public int Status
        {
            get => _status;
            set => _status = value;
        }

        //Làm nhiệm vụ validate account trả về một dictionary các lỗi
        public Dictionary<string, string> CheckValidate()
        {
            AccountModel model = new AccountModel();
            Dictionary<string, string> errors = new Dictionary<string, string>();
            if (string.IsNullOrEmpty(this._username))
            {
                errors.Add("username", "Username can not be null or empty");
            }else if (this._username.Length < 6)
            {
                errors.Add("username", "Username is too short. At least 6 characters.");
            }else if (model.CheckExistUsername(this._username))
            {
                errors.Add("username", "Username is exit.Please try another one");
            }

            if (_cpassword != _password)
            {
                errors.Add("password", "Confirm password does not match");
            }

            return errors;
        }
    }
}