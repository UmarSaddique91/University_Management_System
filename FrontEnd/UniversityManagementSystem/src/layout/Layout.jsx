import React from 'react'
import Logo from '../assets/Logo.png'
import { LayoutDashboard, Users, Folder, FileText, Settings, Search, Bell, LogOut } from 'lucide-react'
import { useLocation,useNavigate } from 'react-router-dom'
import { useState, useEffect, useRef } from 'react'

const Layout = ({ children }) => {
    const location = useLocation();
    const navigate=useNavigate();


    //States and Logic For Notofication

    const [showNotification, setShowNotification] = useState(false);
    const [notifications, setNotificaitons] = useState([
        {
            id: 1,
            message: "Assigned Web Development Course to Sara Khan",
            timeStamp: "04/09/2026, 1:22pm"
        },
        {
            id: 2,
            message: "Assigned Web Development Course to Sara Khan",
            timeStamp: "04/09/2026, 1:22pm"
        }
    ])


    // States for Profile

    const [showProfile,setShowProfile]= useState(false);
    const profileRef= useRef();

    // Close notification dropdown when clicking outside
    const notificationRef = useRef(null);



    // UseEffects For Handel DropDowns 

    useEffect(() => {
        const handleClickOutside = (event) => {
            if (notificationRef.current && !notificationRef.current.contains(event.target)) {
                setShowNotification(false);
            }

            if(profileRef.current && !profileRef.current.contains(event.target)){
                setShowProfile(false);
            };
        };

        document.addEventListener('mousedown', handleClickOutside);
        return () => {
            document.removeEventListener('mousedown', handleClickOutside);
        }


    })

    
        // Logout Handler 

        const handleLogout=(e)=>{
            navigate("/");
        }
    return (

        <div className="h-screen bg-slate-200 flex ">

            {/* Left Side Bar  */}
            <aside>


                <div className="w-64 bg-slate-900 h-full flex flex-col  p-5">

                    <img src={Logo} alt="University Logo" className='w-50 h-20' />

                    {/* Navigation */}

                    <nav className="flex flex-col flex-1 space-y-1 mt-6">

                        <a href="/dashboard"
                            className={`flex items-center text-white gap-2 py-2 px-2 
                rounded rounded-md w-full transition-colors 
                ${location.pathname === '/dashboard' ? 'bg-blue-500' : 'bg-transparent hover:bg-blue-600'}  `}>

                            <LayoutDashboard className="text-white" size={14}/>
                            <span className="text-[14px]">Dashboard</span>
                        </a>

                        <a href="/teachers"
                            className={`flex items-center text-white gap-2 py-2 px-5 
                rounded rounded-md w-full transition-colors 
                ${location.pathname === '/teachers' ? 'bg-blue-500' : 'bg-transparent hover:bg-blue-600'}  `}>

                            <Users className="text-white w-5 h-5" />
                            <span className="text-[1.1rem]">Teachers</span>
                        </a>


                        <a href="/courses"
                            className={`flex items-center text-white gap-2 py-2 px-5 
                rounded rounded-md w-full transition-colors 
                ${location.pathname === '/courses' ? 'bg-blue-500' : 'bg-transparent hover:bg-blue-600'}  `}>

                            <Folder className="text-white w-5 h-5" />
                            <span className="text-[1.1rem]">Courses</span>
                        </a>


                        <a href="/assignments"
                            className={`flex items-center text-white gap-2 py-2 px-5 
                rounded rounded-md w-full transition-colors 
                ${location.pathname === '/assignments' ? 'bg-blue-500' : 'bg-transparent hover:bg-blue-600'}  `}>

                            <FileText className="text-white w-5 h-5" />
                            <span className="text-[1.1rem]">Assignments</span>
                        </a>


                        <a href="/settings"

                            className={`flex items-center text-white gap-2 py-2 px-5 
                rounded rounded-md w-full transition-colors 
                ${location.pathname === '/settings' ? 'bg-blue-500' : 'bg-transparent hover:bg-blue-600'}  `}>

                            <Settings className="text-white w-5 h-5" />
                            <span className="text-[1.1rem]">Settings</span>
                        </a>
                    </nav>
                </div>
            </aside>




            {/* Header */}

            {
                <div className="flex  h-20 bg-white w-full  items-center justify-between px-10 py-4">
                    <div className="relative ">
                        <Search className="absolute text-gray-500 left-3 top-3.5 w-5 h-5" />
                        <input type="text" placeholder="Search teachers,course..."
                            className="bg-slate-200 rounded rounded-md py-3 px-10 w-90 focus:outline-none" />
                    </div>

                    {/* bell and admin icon */}
                    <div className="flex items-center gap-4">

                        {/* Bell Icon for Notifications */}
                        <div
                            onClick={() => setShowNotification(!showNotification)}
                            ref={notificationRef}
                            className="relative rounded rounded-md bg-slate-200 p-2 cursor-pointer hover:bg-slate-300">
                            <Bell className="text-gray-500 w-7 h-7  " />
                            {notifications.length > 0 && <span className="absolute top-2 right-2 w-3 h-3 rounded-full bg-red-500"></span>}
                        </div>


                        {/* Notification DropDown Box */}

                        {showNotification && (
                            <div className="absolute top-13 right-3 mt-3 bg-white border border-gray-300 overflow-hidden rounded rounded-md w-80 p-3">
                                <h2 className='font-bold text-[1.3rem] bg-gray-300 border-b border-black p-4 -mx-4 -mt-4 '>Notifications</h2>


                                <div>
                                    {notifications.length > 0 ? (
                                        notifications.map((notification) => (
                                            <div key={notification.id} className="p-5 border-b border-gray-300 -mx-4">
                                                <div className="font-semibold">{notification.message}</div>
                                                <div className='text-slate-500'>{notification.timeStamp}</div>
                                            </div>
                                        ))
                                    ) : (
                                        <div className="p-3 text-gray-500">No notifications</div>
                                    )}
                                </div>

                                <div className=' bg-gray-300 border-t border-black p-4 -mx-4 -mb-4 text-center '>
                                    <a href="#"
                                        onClick={(e) => {
                                            e.preventDefault();
                                            setNotificaitons([])
                                        }}
                                        className='text-blue-500 underline pb-1 text-[1.2rem]'>Clear All</a>
                                </div>

                            </div>


                        )}


                        {/* Admin Icon and Name */}
                        <div 
                        onClick={()=>setShowProfile(!showProfile)}
                        ref={profileRef}
                        className=" relative flex items-center gap-2 ml-4 cursor-pointer">
                            <div className="w-10 h-10 rounded-full bg-slate-200 w-6 h-6 overflow-hidden">
                                <img src="https://ui-avatars.com/api/?name=Admin+User&background=0D8ABC&color=fff" alt="admin image" />
                            </div>

                            <span className="text-black text-[1.1rem] font-semibold">Admin</span>


                            {/* Logout Drop Down Box  */}

                            {showProfile&&(

                                       <div  className="absolute top-9 right-0 mt-2 bg-white border border-gray-300
                     rounded rounded-md w-60 p-3 overflow-hidden shadow-md ">
                                <div className="flex items-center gap-3 bg-gray-300 p-4 -mx-4 -mt-4 border-b border-black">

                                    <div className="w-10 h-10 rounded-full overflow-hidden ">
                                        <img className="w-full h-full object-cover" src="https://ui-avatars.com/api/?name=Admin+User&background=0D8ABC&color=fff" alt="admin image" />
                                    </div>


                                    <div className="flex flex-col">
                                        <span className="font-semibold text-[1.1rem]">Admin User</span>
                                        <span className="text-gray-600 text-[0.9rem] font-semibold -mt-2">Administrator</span>
                                    </div>

                                    
                                </div>

                                {/* Logout Button  */}

                                    <div>
                                        <button onClick={handleLogout}   className='bg-gray-300 w-full p-3 cursor-pointer rounded rounded-md my-3
                                        flex items-center gap-3
                                        '>
                                            <LogOut className="w-6 h-6 text-gray-800"/>
                                            <p className='text-blue-500 font-semibold text-[1rem]'>LogOut</p>
                                        </button>
                                    </div>

                                    {/* Help&Support  */}

                                    <div className=' bg-gray-300 border-t border-black p-4 -mx-4 -mb-4 text-center '>
                                    <a href="#"
                                        
                                        className='text-blue-500 underline pb-1 text-[1rem]'>Help&Support</a>
                                </div>
                            </div>
                            )}
                     

                        </div>




                    </div>




                </div>
            }
        </div>

    )
}

export default Layout